using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ClosedXML.Excel;
using System;
using System.Linq;
using NVorbis;
using System.IO;

namespace SoundSystem
{
    public class ImportSoundData : EditorWindow
    {
        string ExcelDataFile;

        string inputFolder_BGM;
        string inputFolder_SE;

        string outputFolder;

        [MenuItem("Window/Sound/ImportSoundData")]
        protected static void Init()
        {
            ImportSoundData window = (ImportSoundData)GetWindow(typeof(ImportSoundData));
            window.Show();
        }

        protected void OnGUI()
        {
            var nput = FileUtil.GetProjectRelativePath(ExcelDataFile);
            EditorGUILayout.LabelField($"Input Excel File : {((string.IsNullOrEmpty(nput)) ? ExcelDataFile : nput)}");
            EditorGUILayout.Space();

            var input_r = FileUtil.GetProjectRelativePath(inputFolder_BGM);
            EditorGUILayout.LabelField($"Input Folder Path (BGM) : {((string.IsNullOrEmpty(input_r)) ? inputFolder_BGM : input_r)}");

            var input_r2 = FileUtil.GetProjectRelativePath(inputFolder_SE);
            EditorGUILayout.LabelField($"Input Folder Path (SE) :{((string.IsNullOrEmpty(input_r2)) ? inputFolder_SE : input_r2)}");
            EditorGUILayout.Space();
            var input_r3 = FileUtil.GetProjectRelativePath(outputFolder);
            EditorGUILayout.LabelField($"Output Path :{((string.IsNullOrEmpty(input_r3)) ? outputFolder : input_r3)}");
            EditorGUILayout.Space();
            if (GUILayout.Button("Set SoundData ExcelFile"))
            {
                SetFilePath(ref ExcelDataFile);
            }
            EditorGUILayout.Space();
            if (GUILayout.Button("Set Input BGM Path"))
            {
                SetFolderPath(ref inputFolder_BGM);
            }

            if (GUILayout.Button("Set Input SE Path"))
            {
                SetFolderPath(ref inputFolder_SE);
            }
            EditorGUILayout.Space();
            if (GUILayout.Button("Set Output Path"))
            {
                SetFolderPath(ref outputFolder);
            }
            EditorGUILayout.Space();
            //元のInspector部分の下にボタンを表示
            if (GUILayout.Button("データの設定"))
            {
                Read(ExcelDataFile, inputFolder_BGM, inputFolder_SE, outputFolder);
            }
        }

        void SetFilePath(ref string text)
        {
            text = EditorUtility.OpenFilePanel("Select File and Set Path", Application.dataPath, string.Empty);

        }

        void SetFolderPath(ref string text)
        {
            text = EditorUtility.OpenFolderPanel("Select Folder and Set Path", Application.dataPath, string.Empty);
        }


        void Read(string filepath,string bgm_source,string se_source,string output)
        {
            using var wb = new XLWorkbook(filepath);

            IXLWorksheet bgm;
            IXLWorksheet se;

            wb.TryGetWorksheet("BGM", out bgm);
            wb.TryGetWorksheet("SE", out se);

            if (bgm != null && se != null)
            {
                var readBGM = new ReadBGMSheet(bgm);
                var readSE = new ReadSESheet(se);

                if (!Directory.Exists(Path.Combine(output, "BGM")))
                {
                    Directory.CreateDirectory(Path.Combine(output, "BGM"));
                }

                if (!Directory.Exists(Path.Combine(output, "SE")))
                {
                    Directory.CreateDirectory(Path.Combine(output, "SE"));
                }

                AssetDatabase.Refresh();

                readBGM.Generate(bgm_source, output);
                readSE.Generate(se_source, output);

                GenerateSoundData(output, readBGM.BGMData, readSE.SEData);

            }
            else
            {
                Debug.LogError("you need 2 sheets named 'BGM' and 'SE'.");
            }
        }

        void GenerateSoundData(string output, List<BGMData> bgms, List<SEData> ses)
        {
            var sounddata = ScriptableObject.CreateInstance<SoundData>();

            AssetDatabase.CreateAsset(sounddata,FileUtil.GetProjectRelativePath( Path.Combine(output, "SoundData.asset")));

            sounddata.init(bgms, ses);

            EditorUtility.SetDirty(sounddata);
        }

    }

    public class ReadSESheet : ReadSheet
    {
        public List<List<string>> data = new List<List<string>>();
        public List<SoundSystem.SEData> SEData = new List<SoundSystem.SEData>();

        public ReadSESheet(IXLWorksheet sheet) : base(sheet, 2)
        {
        }
        protected override void Process(List<string> values)
        {
            if (!String.IsNullOrEmpty(values[0]) && !String.IsNullOrEmpty(values[1]))
            {
                data.Add(values);
            }
        }

        public void Generate(string source, string output)
        {
            foreach (var values in data)
            {

                var oldpath = FileUtil.GetProjectRelativePath(Path.Combine(source, values[1]));
                var newpath = FileUtil.GetProjectRelativePath(Path.Combine(output, "SE", values[1]));

               var soundfile = AssetDatabase.LoadAssetAtPath<AudioClip>(oldpath);

                if (soundfile == null)
                {
                    Debug.LogError($"AudioClip null {oldpath}");
                }
                else
                {
                    AssetDatabase.MoveAsset(oldpath, newpath);

                    var sedata = ScriptableObject.CreateInstance<SoundSystem.SEData>();

                    AssetDatabase.CreateAsset(sedata, FileUtil.GetProjectRelativePath(Path.Combine(output, "SE","SE_" + values[0] + ".asset")));

                    sedata.Init(values[0], values[1], soundfile);
                    EditorUtility.SetDirty(sedata);
                    SEData.Add(sedata);

                }
            }
        }
    }

    public class ReadBGMSheet : ReadSheet
    {
        public List<List<string>> data = new List<List<string>>();
        public List<SoundSystem.BGMData> BGMData = new List<SoundSystem.BGMData>();

        public ReadBGMSheet(IXLWorksheet sheet) : base(sheet, 5)
        {
        }

        protected override void Process(List<string> values)
        {
            if (!String.IsNullOrEmpty(values[0]) && !String.IsNullOrEmpty(values[2]))
            {
                data.Add(values);
            }
        }

        public void Generate(string source, string output)
        {
            foreach (var values in data)
            {
                var oldpath = FileUtil.GetProjectRelativePath(Path.Combine(source, values[2]));
                var newpath = FileUtil.GetProjectRelativePath(Path.Combine(output, "BGM", values[2]));

                var soundfile = AssetDatabase.LoadAssetAtPath<AudioClip>(oldpath);

                if (soundfile == null)
                {
                    Debug.LogError($"AudioClip null {oldpath}");
                }
                else
                {

                    var loopdata = new ReadLoopData(Path.GetFullPath(oldpath));

                    AssetDatabase.MoveAsset(oldpath, newpath);

                    var bgmdata = ScriptableObject.CreateInstance<BGMData>();

                    AssetDatabase.CreateAsset(bgmdata, FileUtil.GetProjectRelativePath(Path.Combine(output,"BGM", "BGM_" + values[0] + ".asset")));

                    bgmdata.Init(values[0], values[1], values[2], values[3], values[4], loopdata.hasLoop, loopdata.loop_start, loopdata.loop_length, soundfile);
                    EditorUtility.SetDirty(bgmdata);

                    BGMData.Add(bgmdata);
                }
            }
        }

    }


    public class ReadSheet
    {
        public ReadSheet(IXLWorksheet sheet, int extentX = 1)
        {
            var range = sheet.RangeUsed(XLCellsUsedOptions.All);

            if (range != null)
            {
                var maxY = range.Cells().Max(x => x.Address.RowNumber);

                var minY = 2;

                string getval(int x, int y) => sheet.Cell(y, x).Value.ToString();

                if (maxY - minY >= 0)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        List<string> values = new List<string>();
                        for (int x = 1; x <= extentX; x++)
                        {
                            values.Add(getval(x, y));
                        }
                        Process(values);
                    }
                }
                else
                {
                    Debug.Log("No Data");
                }
            }
        }

        protected virtual void Process(List<string> values)
        {
            string res = "";
            foreach (var item in values)
            {
                res += item;
                res += " ";
            }
            Debug.Log(res);
        }
    }


    public class ReadLoopData
    {
        public bool hasLoop { get; private set; }
        public int loop_start { get; private set; }
        public int loop_length { get; private set; }

        public ReadLoopData(string filepath)
        {
            using (var vorbis = new VorbisReader(filepath))
            {
                hasLoop = false;

                int st;
                int l;

                bool f_st = int.TryParse(vorbis.Tags.GetTagMulti("LOOPSTART").First(), out st);
                bool f_len = int.TryParse(vorbis.Tags.GetTagMulti("LOOPLENGTH").First(), out l);

                loop_start = st;
                loop_length = l;

                bool f_len2 = vorbis.TotalSamples >= loop_start + loop_length;

                hasLoop = f_st && loop_length > 0 && f_len2;

                Debug.Log($"hasLoop:{hasLoop}  read LoopStart:{f_st}   read LoopLength:{f_len} {loop_length}   LoopSetting:{f_len2}");
            }
        }

    }
}
