

string data = "abcdefghijklmnopqrstuvwxyz";

IEnumerator<(int, int)> TryNewLen(int size1, int size2)
{
    yield return (size1, size2);
    var count = (size1 + size2);
    while (true)
    {
        for (int i = 0; i < count; i++)
        {
            yield return ( i, (count - i));
        }

        count++;
    }
}

IEnumerable<(int A, int B)> GetKnightPos((int A, int B) startPos)
{
    var res = new SortedSet<(int A, int B)>() { };

    foreach (var xMn in new List<int> {1, -1})
    {
        foreach (var yMn in new List<int> {1, -1})
        {
            res.Add((A: startPos.A + 2 * xMn, B: startPos.B + 1 * yMn));
            res.Add((A: startPos.A + 1 * xMn, B: startPos.B + 2 * yMn));
        }
    }

    return res.Where(x => (0 <= x.A & x.A < 8) & (0 <= x.B & x.B < 8));
}

(Dictionary<(int A, int B), List<(int A, int B)>> graph, int len) SolutionFinder(
    (int A, int B) start,
    (int A, int B) finish,
    int minSetups = -1,
    List<(int A, int B)>? badPoints = null
)
{
    badPoints ??= new List<(int A, int B)>();
    int setup = 0;
    var graph = new Dictionary<int, Dictionary<(int A, int B ), List<(int A, int B)>>>();
    // Console.WriteLine("-------------------");
    var allPoints = new HashSet<(int A, int B)>() {start};
    IEnumerable<(int A, int B)> currentPoints = new List<(int A, int B)>() {start};
    IEnumerable<(int A, int B)> nextPoints = new HashSet<(int A, int B)>() { };
    while ((!currentPoints.Contains(finish) | minSetups > setup) & currentPoints.Any())
    {
        graph[setup] = new Dictionary<(int A, int B), List<(int A, int B)>>();
        foreach (var currentPoint in currentPoints)
        {
            // Console.WriteLine($"{data[currentPoint.A]}{currentPoint.B + 1}");
            var nextP = GetKnightPos(currentPoint);


            graph[setup][currentPoint] = nextP.ToList();

            nextPoints = nextPoints.Union(nextP);
        }

        // Console.WriteLine("-------------------");

        if (minSetups > setup)
        {
            nextPoints = nextPoints.Except(badPoints);
        }
        else
        {
            nextPoints = nextPoints.Except(allPoints);
        }

        nextPoints = nextPoints.Except(new[] {start});

        currentPoints = nextPoints.ToList();
        nextPoints = new HashSet<(int A, int B)>() { };
        setup++;
    }

    // foreach (var valueTuple in nextPoints)
    // {
    //     Console.WriteLine($"{data[valueTuple.A]}{valueTuple.B + 1}");
    // }

    var reversedGraph = new Dictionary<(int A, int B ), List<(int A, int B)>>();
    var reversedGraphBySteps = new Dictionary<int, Dictionary<(int A, int B ), List<(int A, int B)>>>();
    currentPoints = new List<(int A, int B)>() {finish};
    nextPoints = new HashSet<(int A, int B)>() { };
    for (var i = graph.Count - 1; i >= 0; i--)
    {
        reversedGraphBySteps[i] = new Dictionary<(int A, int B), List<(int A, int B)>>();
        foreach (var keyValuePair in graph[i])
        {
            foreach (var currentPoint in currentPoints)
            {
                if (keyValuePair.Value.Contains(currentPoint))
                {
                    reversedGraph[currentPoint] =
                        reversedGraph.GetValueOrDefault(currentPoint, new List<(int A, int B)>());
                    reversedGraph[currentPoint].Add(keyValuePair.Key);

                    reversedGraphBySteps[i][currentPoint] = reversedGraphBySteps[i]
                        .GetValueOrDefault(currentPoint, new List<(int A, int B)>());
                    reversedGraphBySteps[i][currentPoint].Add(keyValuePair.Key);

                    (nextPoints as HashSet<(int A, int B)>)?.Add(keyValuePair.Key);
                }
            }
        }

        currentPoints = nextPoints.ToList();
        nextPoints = new HashSet<(int A, int B)>() { };
    }

    var d = new Dictionary<int, HashSet<(int A, int B)>>()
    {
        [reversedGraphBySteps.Count] = new HashSet<(int A, int B)>() {finish}
    };
    for (var i = reversedGraphBySteps.Count - 1; i >= 0; i--)
    {
        d[i] = new HashSet<(int A, int B)>();
        foreach (var keyValuePair in reversedGraphBySteps[i])
        {
            d[i] = d[i].Union(keyValuePair.Value).ToHashSet();
        }
    }

    // for (var i = d.Count - 1; i >= 0; i--)
    // {
    //     Console.WriteLine($"{i}: {string.Join(", ", d[i].Select(x => $"{data[x.A]}{x.B + 1}"))}");
    // }

    // Console.WriteLine("==============");
    return (graph: reversedGraph, len: setup);
}


List<(int A, int B)>? GetNextStep(
    Dictionary<(int A, int B), List<(int A, int B)>> stepMap,
    List<(int A, int B)> stepHistory,
    List<(int A, int B)> badPos,
    List<(int A, int B)> NodesShouldUnique
)
{
    (int A, int B)? lastHistoryItem = null;
    var newHistory = stepHistory.Select(x => x).ToList();


    for (var i = stepHistory.Count - 1; i >= 0; i--)
    {
        var historyItem = stepHistory[i];

        List<(int A, int B)> passTryingValues;
        if (i != stepHistory.Count - 1)
        {
            passTryingValues = new List<(int A, int B)>();
            var flag = false;
            foreach (var valueTuple in stepMap[historyItem])
            {
                if (valueTuple == lastHistoryItem)
                {
                    flag = true;
                }
                else if (flag & !NodesShouldUnique.Contains(valueTuple))
                {
                    passTryingValues.Add(valueTuple);
                }
            }
        }
        else
        {
            passTryingValues = stepMap[historyItem].Where(x => !NodesShouldUnique.Contains(x)).ToList();
        }


        foreach (var valueTuple in passTryingValues)
        {
            if (!badPos.Contains(valueTuple))
            {
                newHistory.Add(valueTuple);
                return newHistory;
            }
        }

        lastHistoryItem = historyItem;
        newHistory.Remove(historyItem);
    }

    return null;
}

(List<(int A, int B)>, List<(int A, int B)>) OneSetupSequence(
    List<(int A, int B)> currentHistory,
    (int A, int B) currentFinish,
    List<(int A, int B)> badFieldsForMe,
    List<(int A, int B)> badFieldsForOther,
    Dictionary<(int A, int B), List<(int A, int B)>> currentSetupMap
)
{
    var startedCurrentHistory = currentHistory.Select(x => x).ToList();
    while (currentHistory.Count != 0 & !currentHistory.Contains(currentFinish))
    {
        var lastPos = currentHistory[^1];
        var NodesShouldUnique = new List<(int A, int B)>() { };
        for (var i = startedCurrentHistory.Count - 1; i < currentHistory.Count; i++)
        {
            NodesShouldUnique.Add(currentHistory[i]);
        }

        var newHistory1 = GetNextStep(currentSetupMap, currentHistory, badFieldsForMe, NodesShouldUnique);
        if (newHistory1 == null)
        {
            break;
        }

        currentHistory = newHistory1.Select(x => x).ToList();
        badFieldsForOther.Remove(lastPos);
        badFieldsForOther.Add(currentHistory[^1]);
    }

    return (currentHistory, badFieldsForOther);
}

(List<(int Knight, (int A, int B) Cell)>?, IEnumerator<(int, int)>) GetNotMinimizeAnswer(
    (int A, int B) start1,
    (int A, int B) start2,
    (int A, int B) finish1,
    (int A, int B) finish2,
    IEnumerator<(int, int)>? newLenIterator = null,
    int MaximumSetups = int.MaxValue
)
{
    (var setupMap1, var len1) = SolutionFinder(finish1, start1);
    (var setupMap2, var len2) = SolutionFinder(finish2, start2);

    newLenIterator ??= TryNewLen(len1, len2);
    (var minSetups1, var minSetups2) = newLenIterator.Current;
    newLenIterator.MoveNext();


    var oldLen1 = len1;
    var oldLen2 = len2;
    var badFields1 = new List<(int A, int B)> {start2};
    var badFields2 = new List<(int A, int B)> {start1};

    var history1 = new List<(int A, int B)> {start1};
    var history2 = new List<(int A, int B)> {start2};
    var histiry1TranslateToSequenceIndex = 0;
    var histiry2TranslateToSequenceIndex = 0;


    var lastHistory1 = new List<(int A, int B)> {start1};
    var lastHistory2 = new List<(int A, int B)> {start2};

    var lastSetupsSequence = new List<(int Knight, (int A, int B) Cell)>();
    var setupsSequence = new List<(int Knight, (int A, int B) Cell)>();


    var tryReverse = false;
    var shouldReverse = false;
    var knight1 = 1;
    var knight2 = 2;
    var knightCollisionCount = 0;

    while (history1[^1] != finish1 || history2[^1] != finish2)
    {
        lastSetupsSequence = setupsSequence.Select(x => x).ToList();
        lastHistory1 = history1;
        (history1, badFields2) = OneSetupSequence(history1, finish1, badFields1, badFields2, setupMap1);
        foreach (var valueTuple in history1.Where((i, index) => index > histiry1TranslateToSequenceIndex))
        {
            setupsSequence.Add((Knight: knight1, Cell: valueTuple));
        }

        histiry1TranslateToSequenceIndex = history1.Count - 1;


        lastHistory2 = history2;
        (history2, badFields1) = OneSetupSequence(history2, finish2, badFields2, badFields1, setupMap2);
        foreach (var valueTuple in history2.Where((i, index) => index > histiry2TranslateToSequenceIndex))
        {
            setupsSequence.Add((Knight: knight2, Cell: valueTuple));
        }

        histiry2TranslateToSequenceIndex = history2.Count - 1;

        if ((history1[^1] == finish1 & history2[^1] != finish2 & !tryReverse) | shouldReverse)
        {
            history1 = lastHistory2;
            history2 = lastHistory1;
            (finish1, finish2) = (finish2, finish1);
            (start1, start2) = (start2, start1);
            (badFields1, badFields2) = (badFields2, badFields1);
            (setupMap1, setupMap2) = (setupMap2, setupMap1);
            (knight1, knight2) = (knight2, knight1);
            tryReverse = true;
            setupsSequence = lastSetupsSequence;
            (len1, len2) = (len2, len1);
            (oldLen1, oldLen2) = (oldLen2, oldLen1);
            shouldReverse = false;
        }
        else if ((history1[^1] == finish1 & history2[^1] != finish2 & tryReverse) |
                 (history1[^1] == lastHistory1[^1] & history2[^1] == lastHistory2[^1]))
        {
            // реверс не помог
            // Нужно сделать дополнительный ход
            knightCollisionCount++;
            if (knightCollisionCount % 2 == 1)
            {
                history1 = lastHistory2;
                history2 = lastHistory1;
                (start1, start2) = (start2, start1);
                (finish1, finish2) = (finish2, finish1);
                (badFields1, badFields2) = (badFields2, badFields1);
                (setupMap1, setupMap2) = (setupMap2, setupMap1);
                (knight1, knight2) = (knight2, knight1);
                tryReverse = true;
                setupsSequence = lastSetupsSequence;
                (len1, len2) = (len2, len1);
                (oldLen1, oldLen2) = (oldLen2, oldLen1);
            }
            else if (knightCollisionCount % 2 == 0)
            {
                (minSetups1, minSetups2) = newLenIterator.Current;
                newLenIterator.MoveNext();
            }

            (setupMap1, len1) = SolutionFinder(finish1, start1, minSetups1, new List<(int A, int B)>() {history2[^1]});
            (setupMap2, len2) = SolutionFinder(finish2, start2, minSetups2, new List<(int A, int B)>() {history1[^1]});
            setupsSequence = new List<(int Knight, (int A, int B) Cell)>();
            if (len1 + len2 > MaximumSetups)
            {
                return (null, newLenIterator);
            }


            tryReverse = false;
        }
        else
        {
            tryReverse = false;
        }
    }


    // foreach (var valueTuple in setupsSequence)
    // {  
    //     Console.WriteLine($"{valueTuple.Knight} {data[valueTuple.Cell.A]}{valueTuple.Cell.B + 1}");
    // }
    return (setupsSequence, newLenIterator!);
}

void Main((int A, int B) start1, (int A, int B) start2, (int A, int B) finish1, (int A, int B) finish2)
{
    IEnumerator<(int, int)>? newLenIterator = null;

    (var setupsSequence, newLenIterator) = GetNotMinimizeAnswer(start1, start2, finish1, finish2, newLenIterator, int.MaxValue);

    var minimumSetups = setupsSequence.Count;
    var optimalSequence = setupsSequence.Select(x => x).ToList();
    (var len1, var len2) = newLenIterator.Current;
    var genMinLen = len1 + len2;
    while ((newLenIterator.Current.Item1 + newLenIterator.Current.Item2) <= minimumSetups)
    {
        (setupsSequence, newLenIterator) = GetNotMinimizeAnswer(start1, start2, finish1, finish2, newLenIterator, minimumSetups);
        ( len1,  len2) = newLenIterator.Current;
         genMinLen = len1 + len2;

        if (setupsSequence != null & setupsSequence?.Count < minimumSetups)
        {
            minimumSetups = setupsSequence.Count;
            optimalSequence = setupsSequence.Select(x => x).ToList();
        }
    }
    
    
    foreach (var valueTuple in optimalSequence)
    {  
        Console.WriteLine($"{valueTuple.Knight} {data[valueTuple.Cell.A]}{valueTuple.Cell.B + 1}");
    }
}


string raw_start_1 = Console.ReadLine();
var start1 = (A: data.IndexOf(raw_start_1[0]), B: Convert.ToInt32($"{raw_start_1[1]}") - 1);

string raw_start_2 = Console.ReadLine();
var start2 = (A: data.IndexOf(raw_start_2[0]), B: Convert.ToInt32($"{raw_start_2[1]}") - 1);

string raw_end_1 = Console.ReadLine();
var end1 = (A: data.IndexOf(raw_end_1[0]), B: Convert.ToInt32($"{raw_end_1[1]}") - 1);

string raw_end_2 = Console.ReadLine();
var end2 = (A: data.IndexOf(raw_end_2[0]), B: Convert.ToInt32($"{raw_end_2[1]}") - 1);

// var reversedGraph = SolutionFinder(end1, start1);
// foreach (var keyValuePair in reversedGraph)
// {
//     Console.WriteLine($"{data[keyValuePair.Key.A]}{keyValuePair.Key.B + 1}: {string.Join(", ", keyValuePair.Value.Select(x => $"{data[x.A]}{x.B + 1}"))}");
// }
Main(start1, start2, end1, end2);