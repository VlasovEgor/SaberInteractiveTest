using System.Text;

class Program
{
    static void Main(string[] args)
    {
        ListRand list = new ListRand();

        list.FillingList();
        list.AssigningLinksRand();

        Console.WriteLine("Original list:");
        list.PrintList();

        using (FileStream fileStream = new FileStream("list.dat", FileMode.Create))
        {
            list.Serialize(fileStream);
        }

        ListRand deserializedList = new ListRand();

        using (FileStream fileStream = new FileStream("list.dat", FileMode.Open))
        {
            deserializedList.Deserialize(fileStream);
        }

        Console.WriteLine("Deserialized list:");
        deserializedList.PrintList();
    }
}

class ListNode
{
    public ListNode Prev;
    public ListNode Next;
    public ListNode Rand;
    public string Data;
}

class ListRand
{
    public ListNode Head;
    public ListNode Tail;
    public int Count;

    public void FillingList()
    {
        int minimumNumberOfElements = 1;
        int maximumNumberOfElements = 20;

        int maximumNumberInData = 100;


        Random random = new Random();
        int count = random.Next(minimumNumberOfElements, maximumNumberOfElements);
        Count = count;
        Head = new ListNode { Data = random.Next(maximumNumberInData).ToString() };
        ListNode currentNode = Head;

        for (int i = 1; i < count; i++)
        {
            ListNode node = new ListNode { Data = random.Next(maximumNumberInData).ToString(), Prev = Tail };
            currentNode.Next = node;
            currentNode = node;
        }

    }

    public void AssigningLinksRand()
    {
        Random random = new Random();
        ListNode currentNode = Head;

        while (currentNode != null)
        {
            int randomIndex = random.Next(Count);
            ListNode randomNode = GetNodeAtIndex(Head, randomIndex);
            currentNode.Rand = randomNode;
            currentNode = currentNode.Next;
        }
    }

    private ListNode GetNodeAtIndex(ListNode head, int index)
    {
        if (head == null)
        {
            throw new ArgumentNullException("The head node cannot be null.");
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException("Index cannot be negative.");
        }

        ListNode currentNode = head;

        for (int i = 0; i < index; i++)
        {
            if (currentNode.Next != null)
            {
                currentNode = currentNode.Next;
            }
        }

        return currentNode;
    }

    public void PrintList()
    {
        ListNode current = Head;

        while (current != null)
        {
            Console.WriteLine($"Data: {current.Data}, Rand: {current.Rand.Data}");
            current = current.Next;
        }
    }

    public void Serialize(FileStream fileStream)
    {
        Dictionary<ListNode, int> nodeIndex = new Dictionary<ListNode, int>();

        List<(string, int)> dataList = new List<(string, int)>();

        ListNode current = Head;
        int index = 0;

        while (current != null)
        {
            nodeIndex.Add(current, index);
            current = current.Next;
            index++;
        }

        current = Head;

        while (current != null)
        {
            dataList.Add((current.Data, nodeIndex[current.Rand]));
            current = current.Next;
        }

        byte[] countBytes = BitConverter.GetBytes(Count);
        fileStream.Write(countBytes, 0, countBytes.Length);

        foreach (var (data, randIndex) in dataList)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] dataLengthBytes = BitConverter.GetBytes(dataBytes.Length);
            byte[] randIndexBytes = BitConverter.GetBytes(randIndex);

            fileStream.Write(dataLengthBytes, 0, dataLengthBytes.Length);
            fileStream.Write(dataBytes, 0, dataBytes.Length);
            fileStream.Write(randIndexBytes, 0, randIndexBytes.Length);
        }
    }

    public void Deserialize(FileStream fileStream)
    {

        byte[] countBytes = new byte[4];
        fileStream.Read(countBytes, 0, countBytes.Length);
        Count = BitConverter.ToInt32(countBytes, 0);

        ListNode[] nodes = new ListNode[Count];
        List<int> randIndices = new List<int>();

        for (int i = 0; i < Count; i++)
        {
            byte[] dataLengthBytes = new byte[4];
            fileStream.Read(dataLengthBytes, 0, dataLengthBytes.Length);
            int dataLength = BitConverter.ToInt32(dataLengthBytes, 0);

            byte[] dataBytes = new byte[dataLength];
            fileStream.Read(dataBytes, 0, dataBytes.Length);
            string data = Encoding.UTF8.GetString(dataBytes);

            byte[] randIndexBytes = new byte[4];
            fileStream.Read(randIndexBytes, 0, randIndexBytes.Length);
            int randIndex = BitConverter.ToInt32(randIndexBytes, 0);

            ListNode node = new ListNode();
            node.Data = data;
            nodes[i] = node;

            randIndices.Add(randIndex);
        }

        for (int i = 0; i < Count; i++)
        {
            if (i == 0)
                Head = nodes[i];
            if (i == Count - 1)
                Tail = nodes[i];
            if (i > 0)
                nodes[i].Prev = nodes[i - 1];
            if (i < Count - 1)
                nodes[i].Next = nodes[i + 1];
            if (randIndices[i] != -1)
                nodes[i].Rand = nodes[randIndices[i]];
        }
    }
}