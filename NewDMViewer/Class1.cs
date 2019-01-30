using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BilibiliDM_PluginFramework;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace NewDMViewer
{
    class WSModel
    {
        public string mode;
        public object data;


    }

    class TextModel
    {
        public string provider;
        public string uid;
        public string msg;
        public string username;
        public bool vip;
        public string pre;
        public string post;
    }

    public class GiftModel
    {
        public string provider;
        public string uid;
        public string gift;
        public string gift_id;
        public float count;
        public string username;
        public bool vip;
        public string pre;
        public string post;
    }

    public class BroadCaster : WebSocketBehavior
    {
        public void send(string s)
        {
             Sessions.BroadcastAsync(s, () => { });
        }

    }

    public class NewDMViewer : BilibiliDM_PluginFramework.DMPlugin
    {
        List<BroadCaster> onlineusers=new List<BroadCaster>();
        private HttpServer server;
        BroadCaster caster = new BroadCaster();

        public NewDMViewer()
        {
            this.PluginName = "搞事";
            this.PluginAuth = "CopyLiu";
            this.PluginVer = "0.0.9";
            this.PluginCont = "copyliu@gmail.com";
            this.ReceivedDanmaku += OnReceivedDanmaku;

        }

        public void sendall(string s)
        {
            var r=this.onlineusers.ToList();
            foreach (var broadCaster in r)
            {
                if (broadCaster.State == WebSocketState.Closed)
                {
                    onlineusers.Remove(broadCaster);
                }
                else
                {
                    broadCaster.send(s);
                }
            }
        }


        private void OnReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            if (this.Status)
            {
                switch (e.Danmaku.MsgType)
                {
                    case MsgTypeEnum.Comment:
                    {
                        var obj = new WSModel();
                        obj.mode = "text";
                        var m = new TextModel();
                        m.msg = e.Danmaku.CommentText;
                        m.pre = e.Danmaku.isVIP ? "老爷" : null;
                        m.username = e.Danmaku.UserName;
                        m.vip = e.Danmaku.isVIP;
                        m.provider = "bilibili";
                        m.uid = e.Danmaku.UserID + "";
                        obj.data = m;
                        sendall(JsonConvert.SerializeObject(obj));


                    }
                        break;
                    case MsgTypeEnum.GiftSend:
                    {
                        var obj = new WSModel();
                        obj.mode = "gift";
                        var m = new GiftModel();
                        m.gift = e.Danmaku.GiftName;
                        m.count = e.Danmaku.GiftCount;
                        m.pre = e.Danmaku.isVIP ? "老爷" : null;
                        m.username = e.Danmaku.UserName;
                        m.vip = e.Danmaku.isVIP;
                        m.provider = "bilibili";
                        m.uid = e.Danmaku.UserID + "";
                        obj.data = m;
                        sendall(JsonConvert.SerializeObject(obj));
                    }
                        break;
                }
            }

        }

        public override void Stop()
        {
            this.Log("本插件一旦启用就不能停止(");
        }

        public override void Start()
        {
            var httpsv = new HttpServer(9944);
            httpsv.AddWebSocketService<BroadCaster>("/BB", () =>
            {
                var c=new BroadCaster();
                this.onlineusers.Add(c);
                return c;
            });
            httpsv.Start();
            base.Start();
        }
    }

}
