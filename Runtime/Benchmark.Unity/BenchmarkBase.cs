using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Benchmark.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Benchmark.Unity
{

public abstract class BenchmarkBase : MonoBehaviour
{
	private static     BenchmarkBase?         _instance;
	public             BenchmarkBehaviour?    Prefab;
	public             GUISkin?               Skin;
	private readonly   GUIContent             _content    = new();
	private readonly   List<string>           _history    = new();
	private readonly   StringBuilder          _sb         = new();
	private readonly   List<Statistic>        _statistics = new();
	private            BenchmarkBehaviour?    _behaviour;
	private            IEnumerator<IContext>? _enumerator;
	private            Vector2                _scrollPosition;
	protected abstract IEnumerable<IContext>  Contexts { get; }

	private void Awake()
	{
		if (_instance != null)
			return;

		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		_history.Clear();
		if (!Application.isEditor)
			GarbageCollector.GCMode = GarbageCollector.Mode.Manual;
		DontDestroyOnLoad(gameObject);
		_instance                =  this;
		SceneManager.sceneLoaded += OnSceneLoad;
	}

	private void OnDestroy()
	{
		if (_instance != this)
			return;

		Screen.sleepTimeout      =  SleepTimeout.SystemSetting;
		SceneManager.sceneLoaded -= OnSceneLoad;
		_enumerator?.Dispose();
		_enumerator = null;
		_instance   = null;
		if (!Application.isEditor)
			GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
	}

	private void OnGUI()
	{
		GL.Clear(false, true, Color.clear);

		if (_instance != this)
			return;

		GUI.skin = Skin;

		var position = Vector2.zero;
		foreach (var line in _history)
			Label(line, ref position);

		DrawProgress(ref position);

		if (!Application.isEditor
		 && Button("Close", ref position))
			ReportAndQuit(0);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init() =>
		_instance = null;

	private void DrawProgress(ref Vector2 position)
	{
		if (_behaviour == null)
			return;
		_sb.Clear();
		switch (_behaviour.State)
		{
		case BenchmarkState.None:
			_sb.Append(_behaviour.Name)
			   .Append("...");
			break;
		case BenchmarkState.WarmUp:
			_sb.Append(_behaviour.Name)
			   .Append("... Warmup ")
			   .Append(
					_behaviour.StateProgress.ToString("P")
							  .PadLeft(7))
			   .Append("...");
			break;
		case BenchmarkState.Measurement:
			_sb.Append(_behaviour.Name)
			   .Append("... Measurement ")
			   .Append(
					_behaviour.StateProgress.ToString("P")
							  .PadLeft(7))
			   .Append("...");
			break;
		case BenchmarkState.Done:
			_sb.Append(_behaviour.Name)
			   .Append(". Done!");
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}

		Label(_sb.ToString(), ref position);
	}

	private GUIContent TempContent(string text)
	{
		_content.text    = text;
		_content.image   = null;
		_content.tooltip = string.Empty;
		return _content;
	}

	private void Label(string text, ref Vector2 position)
	{
		var content = TempContent(text);
		var size    = GUI.skin.label.CalcSize(content);
		GUI.Label(new Rect(position, size), content);
		position.y += size.y;
	}

	private bool Button(string text, ref Vector2 position)
	{
		var content = TempContent(text);
		var size    = GUI.skin.button.CalcSize(content);
		var rect    = new Rect(position, size);
		position.y += size.y;
		return GUI.Button(rect, content);
	}

	private void MoveNext(Scene scene, int hash)
	{
		if (_behaviour != null)
		{
			_history.Add($"{_behaviour.Name}. Done! - {hash:X}");
			Destroy(_behaviour.gameObject);
		}

		GC.Collect();
		SceneManager.LoadScene(scene.buildIndex, LoadSceneMode.Single);
	}

	private void OnSceneLoad(Scene scene, LoadSceneMode __)
	{
		if (_enumerator == null)
		{
			_enumerator = Contexts.GetEnumerator();
			MoveNext(scene, 0);
			return;
		}

		if (Prefab == null)
		{
			ReportAndQuit(0);
			return;
		}

		_behaviour = Instantiate(Prefab);
		if (_behaviour == null
		 || !_enumerator.MoveNext())
		{
			ReportAndQuit(0);
			return;
		}

		var context = _enumerator.Current;
		if (context == null)
		{
			MoveNext(scene, 0);
			return;
		}

		_behaviour.Run(
			context,
			(hash, samples) =>
			{
				var statistic = new Statistic(context.ToString(), hash, samples ?? Array.Empty<Sample>());
				_statistics.Add(statistic);
				MoveNext(scene, hash);
			});
	}

	private void ReportAndQuit(int hash)
	{
		if (_behaviour != null)
		{
			_history.Add($"{_behaviour.Name}. Done! - {hash:X}");
			Destroy(_behaviour.gameObject);
			Report(_behaviour.EntityCount, _behaviour.MeasurementCount, _behaviour.IterationCount);
		}

		if (Application.isEditor)
			return;

		Destroy(gameObject);
		Application.Quit();
	}

	private void Report(int entityCount, int measurementCount, int iterationCount)
	{
		if (_statistics.Count == 0)
			return;
		_statistics.Sort((l, r) => l.Average.CompareTo(r.Average));
		var maxNameLength = _statistics.Max(statistic => statistic.Name.Length);
		var maxAverage    = _statistics.Max(statistic => statistic.Average);
		var ranks         = _statistics.Ranks();
		var units = maxAverage.Ticks switch
		{
			> TimeSpan.TicksPerSecond      * 100 => Units.Seconds,
			> TimeSpan.TicksPerMillisecond * 100 => Units.Millisecond,
			> TimeSpan.TicksPerMillisecond / 10  => Units.Microseconds,
			_                                    => Units.Nanoseconds,
		};
		var shortName = units.ToShortName();
		var sb        = new StringBuilder();
		sb.AppendLine("```")
		  .Append("|Rank")
		  .Append("|Name".PadRight(maxNameLength + 1))
		  .AppendFormat("|Average ({0})", shortName)
		  .Append("|Hash".PadRight(9))
		  .AppendLine("|")
		  .Append("|----")
		  .Append("|".PadRight(maxNameLength + 1, '-'))
		  .Append("|---------")
		  .Append(new string('-', shortName.Length))
		  .AppendLine(":|--------|");
		var          count = _statistics.Count;
		const string del   = "|";
		for (var index = 0; index < count; index++)
		{
			var statistic = _statistics[index];
			sb.AppendFormat("|{0,4}", ranks[index])
			  .AppendFormat(del)
			  .Append(statistic.Name.PadRight(maxNameLength))
			  .Append(del)
			  .Append(
				   statistic.Average.To(units)
							.ToString("F", CultureInfo.InvariantCulture)
							.PadLeft(10 + shortName.Length))
			  .Append(del)
			  .Append(
				   statistic.Hash.ToString("X")
							.PadLeft(8, '0'))
			  .AppendLine(del);
		}

		sb.AppendLine("```");

		var path    = Path.Combine(Application.persistentDataPath, "Benchmark.md");
		var content = sb.ToString();
		_history.Add(content);
		using (var stream =
			   new MemoryStream(4 + count * (8 + maxNameLength * 4) + count * measurementCount * Sample.Size))
		{
			using (var brotliStream = new BrotliStream(stream, CompressionLevel.Optimal, true))
			using (var writer = new BinaryWriter(brotliStream, Encoding.UTF8, true))
			{
				writer.Write(count);
				foreach (var statistic in _statistics)
					statistic.WriteTo(writer);
			}

			sb.Append(Convert.ToBase64String(stream.ToArray()));
		}

		var dataContent = sb.ToString();
		File.WriteAllText(path, dataContent);

		if (Application.isEditor)
			Debug.LogFormat("Results: {0}\n{1}", path, dataContent);
		else
		{
			var title = iterationCount > 1
				? $"Benchmark {entityCount}x{measurementCount}x{iterationCount} ({Application.platform})"
				: $"Benchmark {entityCount}x{measurementCount} ({Application.platform})";
			var urlContent = UnityWebRequest.EscapeURL(content);
			Application.OpenURL($"https://t.me/share/url?url={UnityWebRequest.EscapeURL(title)}&text={urlContent}");
		}
	}
}

}
