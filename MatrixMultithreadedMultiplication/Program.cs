using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public class Program
{
    private static List<List<double>> A = new List<List<double>>();
    private static List<List<double>> B = new List<List<double>>();
    private static List<List<double>> C = new List<List<double>>();

    private static readonly object matrixLock = new object();

    public static void computeCell(object coords)
    {
        (int row, int column) = ((int, int))coords;
        double total = 0;
        int n = A[0].Count;

        lock (matrixLock)
        {
            for (int i = 0; i < n; i++)
            {
                total += A[row][i] * B[i][column];
            }

            C[row][column] = total;
        }
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

    public static void mmultSecuential()
    {
        for (int i = 0; i < C.Count; i++)
        {
            for (int j = 0; j < C[i].Count; j++)
            {
                (int, int) coords = (i, j);
                computeCell(coords);
            }
        }
    }

    public static void mmultMultithreaded()
    {
        List<Thread> threads = new List<Thread>();

        for (int i = 0; i < C.Count; i++)
        {
            for (int j = 0; j < C[i].Count; j++)
            {
                (int, int) coords = (i, j);

                ParameterizedThreadStart startHilo = new ParameterizedThreadStart(computeCell);
                Thread hilo = new Thread(startHilo);
                threads.Add(hilo);

                hilo.Start(coords);
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
        int start = 10;
        int end = 100;

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

            Console.WriteLine($"Time taken in sequential: {stopWatch.Elapsed.TotalMilliseconds}ms\n");

        }
    }
}