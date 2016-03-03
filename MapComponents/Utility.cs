using System.Collections;

public static class Utility{

    static int hMax = (int)(14 *256/2), oMax = (int)(14 * 256 / 1.75), pMax = (int)(14 * 256 / 1.5);

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

    public static string[] GenerateGeneString(int seed, int Amount, char spawnerType) {
        System.Random prng = new System.Random(seed * GeneGenerationPass);
        GeneGenerationPass++;
        string[] OutputGene = new string[Amount];
        for (int i = 0; i < Amount; i++)
        {
            do
            {
                OutputGene[i] = "";
                for (int j = 0; j <= 13; j++)
                {
                    switch (j)
                    {
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
                            OutputGene[i] += prng.Next(0x0, 0xf).ToString() + "," + spawnerType + ",";
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
                            OutputGene[i] += prng.Next(0, 3).ToString();
                            break;
                    }
                }
            } while (GeneValidityCheck(OutputGene[i].Split(',')) != true);
        }
            return OutputGene;
    }

    public static bool GeneValidityCheck(string[] GeneToCheck)
    {
        if (GeneToCheck == null || GeneToCheck.Length != 14) { return false; }
        int TotalPointMaximum;
        int normalizedGeneScore = 0;
        switch (GeneToCheck[4].ToCharArray()[0]) {
            case 'h':
                TotalPointMaximum = hMax;
                break;
            case 'o':
                TotalPointMaximum = oMax;
                break;
            case 'p':
                TotalPointMaximum = pMax;
                break;
            default:
                return false;
        }
        normalizedGeneScore += (int.Parse(GeneToCheck[0]) + int.Parse(GeneToCheck[1]) + int.Parse(GeneToCheck[9]) + 3);
        for (int i = 5; i < 12; i++) {
            normalizedGeneScore += ((int.Parse(GeneToCheck[i])+1) * 16);
            if (i == 8) { i++; }
        }
        string[] temp = GeneToCheck[2].Split(':');
        normalizedGeneScore += ((int.Parse(temp[0]) + int.Parse(temp[1]) + 2)/2);
        normalizedGeneScore += (256 / (int.Parse(GeneToCheck[0]) + 1) * 16);
        normalizedGeneScore += ((int.Parse(GeneToCheck[13]) + 1) * 64);
        if (normalizedGeneScore <= TotalPointMaximum) {
            return true; }
        return false;
    }
}
