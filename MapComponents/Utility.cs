using System.Collections;

public static class Utility{

    static int GeneGenerationPass = 1;

    public static T[] ShuffleArray<T>(T[] array, int seed) {
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < array.Length - 1; i++) {
            int randomIndex = prng.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        return array;
    }

    public static string[] GenerateGeneString(int seed, int Amount) {
        System.Random prng = new System.Random(seed * GeneGenerationPass);
        GeneGenerationPass++;
        string[] OutputGene = new string[Amount];
        for (int i = 0; i < Amount; i++) {
            OutputGene[i] = "";
            for (int j = 0; j <= 13; j++) {
                switch (j) {
                    case 0:
                        OutputGene[i] += prng.Next(0x00, 0xff).ToString() + ",";
                        break;
                    case 1:
                        OutputGene[i] += prng.Next(0x00, 0xff).ToString() + ",";
                        break;
                    case 2:
                        OutputGene[i] += prng.Next(0x00, 0xff).ToString() + ":" + prng.Next(0x00, 0xff).ToString() + ",";
                        break;
                    case 3:
                        OutputGene[i] += prng.Next(0x0, 0xf).ToString() + ",!,";
                        break;
                    case 4:
                        OutputGene[i] += prng.Next(0x0, 0xf).ToString() + ",";
                        break;
                    case 5:
                        OutputGene[i] += prng.Next(0x0, 0xf).ToString() + ",";
                        break;
                    case 6:
                        OutputGene[i] += prng.Next(0x0, 0xf).ToString() + ",";
                        break;
                    case 7:
                        OutputGene[i] += prng.Next(0x0, 0xf).ToString() + ",";
                        break;
                    case 8:
                        OutputGene[i] += prng.Next(0x00, 0xff).ToString() + ",";
                        break;
                    case 9:
                        OutputGene[i] += prng.Next(0x0, 0xf).ToString() + ",";
                        break;
                    case 10:
                        OutputGene[i] += prng.Next(0x0, 0xf).ToString() + ",";
                        break;
                    case 11:
                        OutputGene[i] += prng.Next(0x0, 0xf).ToString() + ",";
                        break;
                    case 12:
                        OutputGene[i] += prng.Next(1, 3).ToString();
                        break;
                }
            }
        }
        return OutputGene;
    }

}
