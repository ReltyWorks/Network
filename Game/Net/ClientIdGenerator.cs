using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Net
{
    internal class ClientIdGenerator
    {
        public ClientIdGenerator(int maxClients = 100)
        {
            _idSet = new HashSet<int>();
            _availableIds = new Queue<int>(100);

            for (int i = 1; i <= maxClients; i++)
            {
                _availableIds.Enqueue(i);
            }
        }

        HashSet<int> _idSet;
        Queue<int> _availableIds;

        /// <summary>
        /// 사용가능한 id 할당
        /// </summary>
        /// <returns> -1 : 남은 id 없음 </returns>
        public int AssignClientId()
        {
            if (_availableIds.Count > 0)
            {
                int id = _availableIds.Dequeue();
                _idSet.Add(id);
                return id;
            }
            else
            {
                return -1;
            }
        }

        public void ReleaseClientId(int id)
        {
            if (_idSet.Remove(id))
            {
                _availableIds.Enqueue(id);
            }
            else
            {
                throw new Exception($"{id} 는 발급한적없는데..");
            }
        }
    }
}
