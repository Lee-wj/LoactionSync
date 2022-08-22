using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * 假设游戏角色只有分数需要存入数据库
 * DataMgr类将PlayerData序列化后保存到player表的data栏位中
 **/

namespace GameSever.Logic
{
    [Serializable]
    public class PlayerData
    {
        public int score = 0;
        public PlayerData()
        {
            score = 100;
        }
    }
}
