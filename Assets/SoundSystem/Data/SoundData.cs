using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SoundSystem {
    public class SoundData : ScriptableObject
    {
        [field: SerializeField]
        List<BGMData> BGMData { get; set; }

        [field: SerializeField]
        List<SEData> SEData { get; set; }

        Dictionary<string, BGMData> BGMdict;
        Dictionary<string, SEData> SEdict;

        public Dictionary<string, BGMData> BGM
        {
            get
            {
                if (BGMdict == null)
                {
                    BGMdict = new Dictionary<string, BGMData>();

                    foreach (var item in BGMData)
                    {
                        BGMdict.Add(item.ID, item);
                    }

                    return BGMdict;
                }
                else
                {
                    return BGMdict;
                }
            }
        }
        public Dictionary<string, SEData> SE
        {
            get
            {
                if (SEdict == null)
                {
                    SEdict = new Dictionary<string, SEData>();

                    foreach (var item in SEData)
                    {
                        SEdict.Add(item.ID, item);
                    }

                    return SEdict;
                }
                else
                {
                    return SEdict;
                }
            }
        }



        public void init(List<BGMData> b, List<SEData> s)
        {
            BGMData = b;
            SEData = s;
        }


    }


}

