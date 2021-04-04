using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace InfectionSimulator
{

    // Several rules will be referenced in comments in the format [Rule X] - see the rules as written to be played
    // by a group of humans at: https://docs.google.com/document/d/e/2PACX-1vRCza3qleB18DSKPhnNeoBCPnERwobFMctkESiFxZSMgmagzVemz1Tqr6N0twy3-8347F9CQrZhN72l/pub
    class Program
    {
        static int dayPhase;
        static int nightPhase;
        static int teamSurvivorCount;
        static int teamZombieCount;

        static int zombiesWon;
        static int survivorsWon;

        static List<string> TeamSurvivor = new List<string>();
        static List<string> Enforcer = new List<string>();
        static List<string> BittenPlayers = new List<string>();
        static List<string> OneMoreNight = new List<string>();

        static List<string> PatientZero = new List<string>();
        static List<string> TeamZombie = new List<string>();

        static List<string> ThePlanOfAction = new List<string>();
        static List<string> LastPlanOfAction = new List<string>();

        static List<string> AllPlayersForSelections = new List<string>();
        static List<string> DeadPlayers = new List<string>();

        static List<string> GroupLeaderViews = new List<string>();
        static List<string> EnforcerProtecting = new List<string>();


        static void Main(string[] args)
        {
            int howMany;
            Console.WriteLine("How many simulations would you like to run?");
            do
            {
                Console.WriteLine("Needs to be a whole number. (i.e. 1000)");
            } while (!int.TryParse(Console.ReadLine(), out howMany));
                     
            while (howMany > 0)
            {
                SetUpNewGame();
                StartDayPhase();
                Console.WriteLine($"Simulating game {zombiesWon + survivorsWon}");
                howMany--;
            }
            Console.WriteLine($"Zombies won {zombiesWon} games");
            Console.WriteLine($"Survivors won {survivorsWon} games");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Would you like to run more simulations?");
            Console.WriteLine("If so, hit enter.");
            Console.WriteLine("If not, type 'quit' and hit enter to close.");
            string userChoice = Console.ReadLine().ToLower();
            if (userChoice != "quit")
            {
                zombiesWon = 0;
                survivorsWon = 0;
                Main(args);
            }
            else
            {
                Environment.Exit(0);
            }
        }
        // SetUpNewGame clears all lists and establishes new lists as needed to simulate a new game.
        // It covers [Rule A] & [Rule A1] & [Rule D] & [Rule E]
        static void SetUpNewGame()
        {
            dayPhase = 0;
            nightPhase = 0;

            List<string> PotentialPlayers = new List<string> {"Player1", "Player2", "Player3",
                "Player4", "Player5", "Player6", "Player7", "Player8",
                "Player9", "Player10", "Player11", "Player12", "Player13",
                "Player14", "Player15", "Player16", "Player17", "Player18",
                "Player19", "Player20", "Player21" };

            TeamSurvivor.Clear();
            Enforcer.Clear();
            BittenPlayers.Clear();
            PatientZero.Clear();
            TeamZombie.Clear();
            AllPlayersForSelections.Clear();
            GroupLeaderViews.Clear();
            DeadPlayers.Clear();
            ThePlanOfAction.Clear();
            LastPlanOfAction.Clear();

            int sudoPlayers = StaticRandom.Instance.Next(10, 22);

            List<string> PlayersThisGame = PotentialPlayers.GetRange(0, sudoPlayers);

            // Subtracting one extra from sudoPlayers each line to prevent out of index errors.
            int findAnEnforcer = StaticRandom.Instance.Next(0, sudoPlayers);
            int findPatientZero = StaticRandom.Instance.Next(0, sudoPlayers - 1);
            int findFirstZombie = StaticRandom.Instance.Next(0, sudoPlayers - 2);
            int firstBite = StaticRandom.Instance.Next(0, sudoPlayers - 3);

            Enforcer.Insert(0, PlayersThisGame.ElementAt(findAnEnforcer));
            PlayersThisGame.RemoveAt(findAnEnforcer);

            PatientZero.Insert(0, PlayersThisGame.ElementAt(findPatientZero));
            PlayersThisGame.RemoveAt(findPatientZero);

            TeamZombie.Insert(0, PlayersThisGame.ElementAt(findFirstZombie));
            PlayersThisGame.RemoveAt(findFirstZombie);

            BittenPlayers.Insert(0, PlayersThisGame.ElementAt(firstBite));
            PlayersThisGame.RemoveAt(firstBite);

            TeamSurvivor.AddRange(PlayersThisGame);
            PlayersThisGame.Clear();

            AllLivePlayers();
        }

        static void UpdateTeamCount()
        {           
            teamSurvivorCount = TeamSurvivor.Count + Enforcer.Count + BittenPlayers.Count + OneMoreNight.Count;

            teamZombieCount = TeamZombie.Count + PatientZero.Count;
        }
        // Keeps the lists clean and free from duplications.
        static void AllLivePlayers()
        {
            AllPlayersForSelections.Clear();
            AllDeadPlayers();
            AllPlayersForSelections.AddRange(TeamSurvivor);
            AllPlayersForSelections.AddRange(TeamZombie);
            AllPlayersForSelections.AddRange(BittenPlayers);
            AllPlayersForSelections.AddRange(PatientZero);
            AllPlayersForSelections.AddRange(Enforcer);
            UpdateTeamCount();
        }
        // Keeps the lists clean and free from duplications. Makes sure dead players stay dead :)
        static void AllDeadPlayers()
        {
            foreach(string i in DeadPlayers)
            {
                TeamSurvivor.Remove(i);
                TeamZombie.Remove(i);
                BittenPlayers.Remove(i);
                PatientZero.Remove(i);
                Enforcer.Remove(i);
            }
        }
        // Covers [Rule B] & [Rule B1]
        static void StartDayPhase()
        {
            UpdateTeamCount();

            if (dayPhase == 0)
            {
                PlanOfAction();
                dayPhase++;
            }
            else
            {
                LynchPlayer();
                PlanOfAction();
                dayPhase++;
            }
            
            BittenPlayers.AddRange(OneMoreNight);
            OneMoreNight.Clear();
            CheckForWinner();
        }
        // Covers [Rule C]
        static void StartNightPhase()
        {
            EnforcerProtects();
            BecomeZombies();
            ZombiesBite();
            ViewPlayers();
            InstaKill();
            nightPhase++;
            CheckForWinner();
        }
        // [Rule D1.1] The Group Leader instantly kills any infected players if they view them during night phase
        private static void InstaKill()
        {
            if (GroupLeaderViews.Any(x => BittenPlayers.Any(y => y == x)))
            {
                var instaKill = GroupLeaderViews.Intersect(BittenPlayers);

                foreach (var x in instaKill)
                {
                    string y = x.ToString();
                    DeadPlayers.Add(y);
                    break;
                }
                AllLivePlayers();
            }
            else
            {
                AllLivePlayers();
            }
        }
        // [Rule G6] if "Patrol" is the active Plan of Action the Group Leader get's addition views.
        private static void ViewPlayers()
        {
            if (ThePlanOfAction.ElementAt(0) == "Patrol")
            {

                if (Enforcer.Count == 1)
                {
                    int views = 4;

                    while (views > 0)
                    {
                        int viewplayer = StaticRandom.Instance.Next(0, AllPlayersForSelections.Count);
                        GroupLeaderViews.Add(AllPlayersForSelections.ElementAt(viewplayer));
                        views--;                       
                    }
                }
                else
                {
                    int views = 3;

                    while (views > 0)
                    {
                        int viewplayer = StaticRandom.Instance.Next(0, AllPlayersForSelections.Count);
                        GroupLeaderViews.Add(AllPlayersForSelections.ElementAt(viewplayer));
                        views--;
                    }
                }
            }
            else
            {
                int views = 2;

                while (views > 0)
                {
                    int viewplayer = StaticRandom.Instance.Next(0, AllPlayersForSelections.Count);
                    GroupLeaderViews.Add(AllPlayersForSelections.ElementAt(viewplayer));
                    views--;
                }
            }
        }

        private static void ZombiesBite()
        {
            // No condition to check if Patient Zero is alive [Rule G2] because that is handled in the
            // PlanOfAction() method.
            // Covers [Rule G2]
            if (ThePlanOfAction.ElementAt(0) == "Shelter")
            {
                int zombiesBite = 1;
                while (zombiesBite > 0)
                {
                    int x = StaticRandom.Instance.Next(0, AllPlayersForSelections.Count);

                    BittenPlayers.Add(AllPlayersForSelections.ElementAt(x));
                    zombiesBite--;
                }
                //[Rule D3]
                foreach (string i in EnforcerProtecting)
                {
                    BittenPlayers.Remove(i);
                }
            }
            // Covers [Rule G4]
            else if (ThePlanOfAction.ElementAt(0) == "Set Alarms")
            {
                if (teamZombieCount > 1)
                {
                    double simulateAlarms = teamZombieCount * .5;
                    int zombiesBite = Convert.ToInt32(simulateAlarms);                 

                    while (zombiesBite > 0)
                    {
                        int x = StaticRandom.Instance.Next(0, AllPlayersForSelections.Count);

                        BittenPlayers.Add(AllPlayersForSelections.ElementAt(x));
                        zombiesBite--;
                        
                    }
                }
                else
                {
                    // If there is only 1 zombie there is a 50% chance they will successfully bite.
                    // StaticRandom doesn't pick the last number, fiftyFifty will either be 1 or 2.
                    int fiftyFifty = StaticRandom.Instance.Next(1, 3);
                    if (fiftyFifty == 1)
                    {
                        int x = StaticRandom.Instance.Next(0, AllPlayersForSelections.Count);
                        BittenPlayers.Add(AllPlayersForSelections.ElementAt(x));
                    }
                }
                //[Rule D3]
                foreach (string i in EnforcerProtecting)
                {
                    BittenPlayers.Remove(i);
                }
            }
            else
            {
                int zombiesBite = 1 * teamZombieCount;
                
                while (zombiesBite > 0)
                {
                    int x = StaticRandom.Instance.Next(0, AllPlayersForSelections.Count);

                    BittenPlayers.Add(AllPlayersForSelections.ElementAt(x));
                    zombiesBite--;
                }
                //[Rule D3]
                foreach (string i in EnforcerProtecting)
                {
                    BittenPlayers.Remove(i);
                }
            }
            // This checks the other condition of [Rule D4] as more than one Regular Zombie targeting
            // the enforcer can get through the 'Auto-protect'. This does not perfectly simulate the
            // chances it was in fact the Enforcer role who was targeted by multiple Zombie's. However
            // for the purposes of this simulation to test the balance of the game I deem it to be a close
            // enough approximation.
            int enforcerBittenChance = StaticRandom.Instance.Next(1, teamZombieCount + 1);
            if (enforcerBittenChance != 1 && BittenPlayers.Count != BittenPlayers.Distinct().Count())
            {
                foreach (string i in Enforcer)
                {
                    BittenPlayers.Remove(i);
                }
            }

            EnforcerProtecting.Clear();

            // This removes duplicates from the bitelist, multiple zombies may bite the same player, but having
            // duplicates will falsly infate team numbers.
            List<string> UniqueBiteList = BittenPlayers.Distinct().ToList();
            BittenPlayers.Clear();
            BittenPlayers.AddRange(UniqueBiteList);
            UniqueBiteList.Clear();
        }
        // Covers [Rule E2]
        private static void BecomeZombies()
        {
            // Simulates chance Group Leader becomes Zombie the and views are lost. The next Group Leader would
            // start thier own list. Doesn't perfectly reflect a human game, as a human player might share their
            // list before being recruited to Team Zombie. [Rule D2]
            int groupLeaderChance = StaticRandom.Instance.Next(1, AllPlayersForSelections.Count + 1);

            if (BittenPlayers.Count > 0 && groupLeaderChance == 1) 
            {
                GroupLeaderViews.Clear();
            }
            // Covers [Rule G3]. Instead of checking each bite against a 50% chance, the simulator takes half of
            // the bitten players and adds them to the OneMoreNight list. Again it doesn't perfectly simulate a
            // game played by humans. I deem it to be an acceptable approximation.
            if (ThePlanOfAction.ElementAt(0) == "Give Medicine")
            {
                double saved = BittenPlayers.Count * .5;
                int savedInt = Convert.ToInt32(saved);

                OneMoreNight = BittenPlayers.GetRange(0, savedInt);
                BittenPlayers.RemoveRange(0, savedInt);
                
                TeamZombie.AddRange(BittenPlayers);
                BittenPlayers.Clear();
            }
            else
            {
                TeamZombie.AddRange(BittenPlayers);
                BittenPlayers.Clear();
            }
        }

        private static void EnforcerProtects()
        {
            if (Enforcer.Count == 1)// Checks to maker sure the Enforcer is alive/active in game.
            {
                // Covers [Rule G5]
                if (ThePlanOfAction.ElementAt(0) == "Take Stims")
                {
                    for (int i = 3; i > 0; i--)
                    {
                        int protecting = StaticRandom.Instance.Next(0, AllPlayersForSelections.Count);
                        EnforcerProtecting.Insert(0, AllPlayersForSelections.ElementAt(protecting));
                    }
                }
                else
                {
                    for (int i = 2; i > 0; i--)
                    {
                        int protecting = StaticRandom.Instance.Next(0, AllPlayersForSelections.Count);
                        EnforcerProtecting.Insert(0, AllPlayersForSelections.ElementAt(protecting));
                    }
                }
            }
        }

        static void PlanOfAction() // [Rule G] & [RuleG1]
        {
            int pickAPlan;
            List<string> PlansOfActionsList;
            PickAPlan(out pickAPlan, out PlansOfActionsList);
            // [Rule G1] no plan of action can be chosen twice in a row. Checks to see if the randomly selected
            // plan of action is the same as the last. If so this method is run repeatedly until they do not
            // match.
            if (dayPhase > 1)
            {
                if (ThePlanOfAction.ElementAt(0) == LastPlanOfAction.ElementAt(0))
                {
                    do
                    {
                        PickAPlan(out pickAPlan, out PlansOfActionsList);
                    } while (ThePlanOfAction.ElementAt(0) == LastPlanOfAction.ElementAt(0));                    
                }
            }
            LastPlanOfAction.Clear();
            LastPlanOfAction.Insert(0, PlansOfActionsList.ElementAt(pickAPlan));
        }

        private static void PickAPlan(out int pickAPlan, out List<string> PlansOfActionsList)
        {
            PlansOfActionsList = new List<string> { "Shelter", "Give Medicine", "Set Alarms",
                "Take Stims", "Patrol"};
            ThePlanOfAction.Clear();
            if (PatientZero.Count == 0)
            {
                if (TeamZombie.Count >= 3)
                {
                    pickAPlan = StaticRandom.Instance.Next(1, 5);
                }
                else
                {
                    pickAPlan = StaticRandom.Instance.Next(1, 4);
                }
            }
            else
            {
                if (TeamZombie.Count >= 3)
                {
                    pickAPlan = StaticRandom.Instance.Next(0, 5);
                }
                else
                {
                    pickAPlan = StaticRandom.Instance.Next(0, 4);
                }
            }
            ThePlanOfAction.Insert(0, PlansOfActionsList.ElementAt(pickAPlan));
        }
        // Includes "smart" lynching. If a human player found the identity of Patient Zero or a Zombie they
        // would incite team survivor to lynch them. If the identity of Patient Zero or a Zombie isn't "known"
        // (on the GroupLeaderViews list), then a random player is lynched. [Rule B2]
        static void LynchPlayer()
        {
            if (GroupLeaderViews.Any(x => PatientZero.Any(y => y == x)))
            {
                DeadPlayers.AddRange(PatientZero);
                AllLivePlayers();
            }
            else if (GroupLeaderViews.Any(x => TeamZombie.Any(y => y == x)))
            {
                var death = GroupLeaderViews.Intersect(TeamZombie);

                foreach (var x in death)
                {
                    string y = x.ToString();
                    DeadPlayers.Add(y);
                    break;
                }
                AllLivePlayers();
            }
            else
            {
                int lynchPlayer = StaticRandom.Instance.Next(0, (AllPlayersForSelections.Count));
                DeadPlayers.Add(AllPlayersForSelections.ElementAt(lynchPlayer));
                AllLivePlayers();
            }
        }
        // Covers [Rule F] Checks if either team won. If so, the loop is ended and a new game is setup, otherwise 
        // the game loop continues.
        static void CheckForWinner()
        {
            if (teamZombieCount >= teamSurvivorCount)
            {
                zombiesWon += 1;
            }
            else if (teamZombieCount == 0)
            {
                survivorsWon += 1;
            }
            else
            {
                if (dayPhase > nightPhase)
                {
                    StartNightPhase();
                }
                else
                {
                    StartDayPhase();
                }
            }
        }

        // Credit to Phil on stackoverflow.com for the StaticRandom class. I do not know if they wrote this class
        // themselves or if they were simply sharing it.
        // https://stackoverflow.com/users/581504/phil
        public static class StaticRandom
        {
            private static int seed;

            private static ThreadLocal<Random> threadLocal = new ThreadLocal<Random>
                (() => new Random(Interlocked.Increment(ref seed)));

            static StaticRandom()
            {
                seed = Environment.TickCount;
            }

            public static Random Instance { get { return threadLocal.Value; } }
        }

        // This method was used while debugging the code. It made it easy to see where I was making incorrect
        // changes to game lists. I've kept it in for debugging potential future updates. However it currently 
        // isn't called anywhere.
        static void Debug(string methodName)
        {

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Changes reflect the {methodName} method.");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("This is the dayPhase int:");
            Console.WriteLine(dayPhase);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("This is the nightPhase int:");
            Console.WriteLine(nightPhase);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("This is the teamSurvivorCount int:");
            Console.WriteLine(teamSurvivorCount);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("This is the teamZombieCount int:");
            Console.WriteLine(teamZombieCount);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("This is the zombiesWon int:");
            Console.WriteLine(zombiesWon);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("This is the survivorsWon int:");
            Console.WriteLine(survivorsWon);
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("This is the TeamSurvivor List:");
            foreach (string i in TeamSurvivor)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("This is the Enforcer List:");
            foreach (string i in Enforcer)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("This is the BittenPlayers List:");
            foreach (string i in BittenPlayers)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("This is the OneMoreNight List:");
            foreach (string i in OneMoreNight)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("This is the PatientZero List:");
            foreach (string i in PatientZero)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("This is the TeamZombie List:");
            foreach (string i in TeamZombie)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("This is the ThePlanOfAction List:");
            foreach (string i in ThePlanOfAction)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("This is the LastPlanOfAction List:");
            foreach (string i in LastPlanOfAction)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("This is the DeadPlayers List:");
            foreach (string i in DeadPlayers)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("This is the GroupLeaderViews List:");
            foreach (string i in GroupLeaderViews)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("This is the EnforcerProtecting List:");
            foreach (string i in EnforcerProtecting)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("This is the AllPlayersForSelections List:");
            foreach (string i in AllPlayersForSelections)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
