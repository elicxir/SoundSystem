using System;
using UnityEngine;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Audio;


namespace SoundSystem
{
    public class InvalidIDException : Exception { }


    public class SoundManager : MonoBehaviour, ISoundManager
    {
        [SerializeField] SoundData SoundData;

        [SerializeField] AudioSource audioSource_BGM;
        [SerializeField] GameObject SE_root;
        [SerializeField] AudioSource[] audioSource_SE;

        string nowPlayingID;

        bool isFirstInit = true;

        [SerializeField] private AudioMixer mixer;

        SoundSettings settings;
        CancellationTokenSource BGM_cancellToken;

        void ISoundManager.SetSettings(SoundSettings settings)
        {
            SetSettings(settings);
        }

        void SetSettings(SoundSettings s)
        {
            settings = s;
            mixer.SetFloat("Master_vol", Mathf.Clamp(20f * Mathf.Log10(settings.Master), -80f, 0f));
            mixer.SetFloat("BGM_vol", Mathf.Clamp(20f * Mathf.Log10(settings.BGM), -80f, 0f));
            mixer.SetFloat("SE_vol", Mathf.Clamp(20f * Mathf.Log10(settings.SE), -80f, 0f));
        }

        bool ISoundManager.isPlayingBGM(string id)
        {
            return id == nowPlayingID;
        }

        void ISoundManager.Init(SoundSettings settings)
        {
            Init(settings);
        }

        void Init(SoundSettings set)
        {
            if (isFirstInit)
            {
                isFirstInit = false;
                settings = set;
                audioSource_SE = SE_root.GetComponentsInChildren<AudioSource>();
            }
            else
            {
                Debug.LogError("you can Init() only once.");
            }

        }

        void ISoundManager.PlayBGM(string id,bool overrideFlag, float fadein, float fadeout)
        {
            if (SoundData.BGM.ContainsKey(id))
            {
                if (overrideFlag || nowPlayingID != id)
                {
                    playBGM(id, fadein, fadeout);
                }
            }
            else
            {
                throw new InvalidIDException();
            }
        }

        void ISoundManager.PlaySE(string id, Vector2? r_pos)
        {
            Vector2 R = r_pos ?? Vector2.zero;

            if (SoundData.SE.ContainsKey(id))
            {
                playSE(id, R);
            }
            else
            {
                throw new InvalidIDException();
            }

        }

        void playSE(string id, Vector2 r_pos)
        {
            var se = SoundData.SE[id];

            AudioSource SE;
            if (GetSEsource(out SE))
            {
                SE.transform.localPosition = r_pos;
                SE.clip = se.Audio;
                SE.Play();
            }
            else
            {
                Debug.LogWarning("out of se source");
            }
        }
        bool GetSEsource(out AudioSource se)
        {
            var t = audioSource_SE.FirstOrDefault(x => x.isPlaying == false);

            se = t;
            return t != null;
        }


        async void playBGM(string id, float FadeINtime = 0, float FadeOUTtime = 0)
        {
            if (audioSource_BGM.isPlaying)
            {
                StopBGM(FadeOUTtime);
            }

            var bgm = SoundData.BGM[id];

            var cts = new CancellationTokenSource();
            BGM_cancellToken = cts;

            audioSource_BGM.clip = bgm.Audio;
            audioSource_BGM.volume = 1;
            if (FadeINtime > 0)
            {
                audioSource_BGM.volume = 0;
                FadeIn(FadeINtime);
            }
            audioSource_BGM.Play();
            nowPlayingID = id;
            LoopSystem(bgm.LoopStart, bgm.LoopLength);

        }


        async UniTask FadeIn(float time)
        {
            float timer = 0;

            while (timer < time)
            {
                timer += Time.deltaTime;
                timer = Mathf.Clamp((timer), 0, time);
                var progress = timer / time;

                audioSource_BGM.volume = progress;
                await UniTask.NextFrame();
            }
            return;
        }

        async UniTask FadeOut(float time)
        {
            float timer = 0;

            while (timer < time)
            {
                timer += Time.deltaTime;
                timer = Mathf.Clamp((timer), 0, time);
                var progress = 1 - timer / time;

                audioSource_BGM.volume = progress;

                await UniTask.NextFrame();
            }
            return;
        }

        async void LoopSystem(int LoopStart, int LoopLengthSamples)
        {
            var source = audioSource_BGM;

            try
            {
                while (true)
                {
                    if (source.timeSamples >= LoopStart + LoopLengthSamples)
                    {
                        source.timeSamples -= LoopLengthSamples;
                        source.Play();
                    }
                    await UniTask.NextFrame(PlayerLoopTiming.Update, BGM_cancellToken.Token);
                }
            }
            catch
            {
                return;
            }
        }

        void ISoundManager.StopBGM(float fadeout)
        {
            StopBGM(fadeout);
        }

        async void StopBGM(float FadeOUTtime = 0)
        {
            if (FadeOUTtime > 0)
            {
                await FadeOut(FadeOUTtime);
            }

            audioSource_BGM.Stop();
            nowPlayingID = string.Empty;
            BGM_cancellToken.Cancel();
        }


    }

    public interface ISoundManager
    {
        void Init(SoundSettings settings);
        void PlayBGM(string id,bool overrideFlag=false, float fadein = 0, float fadeout = 0);

        void PlaySE(string id, Vector2? r_pos = null);

        void StopBGM(float fadeout = 0);

        void SetSettings(SoundSettings settings);

        bool isPlayingBGM(string id);

    }



    public struct SoundSettings
    {

        public static SoundSettings Default
        {
            get
            {
                var e = new SoundSettings();

                e.BGM = 1;
                e.SE = 1;
                e.Master = 1;

                return e;
            }

        }

        //volume‚Í0-1
        public float Master;
        public float BGM;
        public float SE;
    }
}
