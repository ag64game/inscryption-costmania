using GBC;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StressCost.Cost
{
    internal class ValorRankText : PixelText
    {
        public int? value = 0;

        void Awake()
        {
            //base.SetColor(new Color(153f, 188f, 188f));
        }

        void Update()
        {
            if (value != null) base.mainText.text = Convert.ToString(value);
            else base.mainText.text = "";
        }
    }
}
