namespace ChatServer
{
    internal class ClientIdGenerator
    {
        private HashSet<int> _idSet;
        private Queue<int> _availableIds;

        public ClientIdGenerator(int maxClient = 100)
        {
            _idSet = new HashSet<int>();
            _availableIds = new Queue<int>(maxClient);

            for (int i = 0; i < maxClient; i++)
                _availableIds.Enqueue(i);
        }

        /// <summary> 사용가능한 id 할당 </summary>
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
                throw new Exception($"{id}는 발급한적없는데..");
            }
        }
    }
}
