using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public class Program
{
    private static List<List<double>> A = new List<List<double>>();
    private static List<List<double>> B = new List<List<double>>();
    private static List<List<double>> C = new List<List<double>>();

    // private static readonly object matrixLock = new object();

    public static void computeCell(object coords)
    {
        (int row, int column) = ((int, int))coords;
        double total = 0;
        int n = A[0].Count;

        for (int i = 0; i < n; i++)
        {
            total += A[row][i] * B[i][column];
        }

        C[row][column] = total;

    }
    public static void generateRandomMatrix(List<List<double>> M, int size)
    {
        M.Clear();
        Random random = new Random();

        for (int i = 0; i < size; i++)
        {
            M.Add(new List<double>());
            for (int j = 0; j < size; j++)
            {
                M[i].Add(random.NextDouble() * 100);
            }
        }
    }
    public static void generateOnesMatrix(List<List<double>> M, int size)
    {
        M.Clear();
        Random random = new Random();

        for (int i = 0; i < size; i++)
        {
            M.Add(new List<double>());
            for (int j = 0; j < size; j++)
            {
                M[i].Add(1);
            }
        }
    }

    public static void generateZeroesMatrix(List<List<double>> M, int size)
    {
        M.Clear();
        Random random = new Random();

        for (int i = 0; i < size; i++)
        {
            M.Add(new List<double>());
            for (int j = 0; j < size; j++)
            {
                M[i].Add(0);
            }
        }
    }

    public static void printMatrix(List<List<double>> M)
    {
        for (int i = 0; i < M.Count; i++)
        {
            for (int j = 0; j < M[i].Count; j++)
            {
                Console.Write($"{M[i][j]} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    public static void mmultSecuential()
    {
        for (int i = 0; i < C.Count; i++)
        {
            for (int j = 0; j < C[i].Count; j++)
            {
                computeCell((i,j));
            }
        }
    }
    public static void mmultMultithreaded2()
    {
        int chunkSize = 10; // Experiment with different chunk sizes
        int totalCells = C.Count * C[0].Count;

        for (int chunkStart = 0; chunkStart < totalCells; chunkStart += chunkSize)
        {
            int chunkEnd = Math.Min(chunkStart + chunkSize, totalCells);

            ManualResetEvent[] doneEvents = new ManualResetEvent[chunkEnd - chunkStart];

            for (int i = chunkStart; i < chunkEnd; i++)
            {
                int row = i / C[0].Count;
                int column = i % C[0].Count;

                doneEvents[i - chunkStart] = new ManualResetEvent(false);

                ThreadPool.QueueUserWorkItem((state) =>
                {
                    computeCell((row, column));
                    doneEvents[(int)state].Set();
                }, i - chunkStart);
            }

            WaitHandle.WaitAll(doneEvents);
        }
    }

    public static void mmultMultithreaded()
    {
        List<Thread> threads = new List<Thread>();

        for (int i = 0; i < C.Count; i++)
        {
            for (int j = 0; j < C[i].Count; j++)
            {
                ParameterizedThreadStart startHilo = new ParameterizedThreadStart(computeCell);
                Thread hilo = new Thread(startHilo);
                threads.Add(hilo);
                hilo.Start((i, j));
            }
        }

        // Sync
        foreach (Thread thread in threads)
        {
            thread.Join();
        }
    }

    public static void Main()
    {
        generateOnesMatrix(A, 5);
        generateOnesMatrix(B, 5);
        generateZeroesMatrix(C, 5);

        printMatrix(A);
        printMatrix(B);

        mmultMultithreaded();
        printMatrix(C);

        int start = 10;
        int end = 10000;

        for (int i = start; i <= end; i *= 10)
        {
            Console.WriteLine($"N = {i}:");

            generateRandomMatrix(A, i);
            generateRandomMatrix(B, i);
            generateZeroesMatrix(C, i);

            Stopwatch stopWatch = Stopwatch.StartNew();
            mmultSecuential();
            stopWatch.Stop();

            Console.WriteLine($"Time taken in sequential: {stopWatch.Elapsed.TotalMilliseconds}ms");

            generateZeroesMatrix(C, i);
            stopWatch.Restart();
            mmultMultithreaded();
            stopWatch.Stop();

            Console.WriteLine($"Time taken in multithreaded: {stopWatch.Elapsed.TotalMilliseconds}ms\n");

        }
    }
}