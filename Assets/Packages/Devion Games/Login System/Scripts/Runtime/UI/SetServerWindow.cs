using DevionGames.UIWidgets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DevionGames.LoginSystem
{
    public class SetServerWindow : UIWidget
    {

        [Header("Reference")]
        /// <summary>
		/// Referenced UI field
		/// </summary>
		[SerializeField]
        protected InputField ip;
        /// <summary>
        /// Referenced UI field
        /// </summary>
        [SerializeField]
        protected InputField port;

        public void SetIp(string ip)
        {
            ServerLink.instance.SetIP(ip);
        }
    }
}