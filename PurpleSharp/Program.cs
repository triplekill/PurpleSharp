﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using PurpleSharp.Lib;


namespace PurpleSharp
{
    class Program
    {

        static void Usage()
        {
            Console.WriteLine("\n  PurpleSharp Usage:\n");
            Console.WriteLine("\tPurpleSharp.exe /List                    -   Roast all users in current domain");
            Console.WriteLine("\tPurpleSharp.exe /T [Technique_ID]        -   Roast all users in current domain using alternate creds");
        }



        public static void Main(string[] args)
        {
            string techniques, pwd, command, rhost, domain, ruser, rpwd, scoutfpath, simrpath, log, dc, pb_file, nav_action, navfile, scout_action;
            int usertype, hosttype, protocol, pbsleep, tsleep, type, nusers, nhosts;
            pbsleep = tsleep = 0;
            usertype = hosttype = protocol = type = 1;
            nusers = nhosts = 5;
            bool cleanup = true;
            bool opsec = false;
            bool verbose = false;
            bool scoutservice = false;
            bool simservice = false;
            bool newchild = false;
            bool scout = false;
            bool remote = false;
            bool navigator = false;
            techniques = rhost = domain = ruser = rpwd = dc = pb_file = nav_action = navfile = scout_action = "";

            scoutfpath = "C:\\Windows\\Temp\\PurpleSharp.exe";
            simrpath = "Downloads\\Firefox_Installer.exe";
            log = "0001.dat";
            command = "ipconfig.exe";
            pwd = "Summer2019!";

            //should move this to sqlite
            string[] execution = new string[] { "T1117", "T1059", "T1064", "T1086", "T1197", "T1121", "T1035", "T1118" };
            string[] persistence = new string[] { "T1053", "T1136", "T1050", "T1060", "T1084" };
            string[] privelege_escalation = new string[] { "T1053", "T1050" };
            string[] defense_evasion = new string[] { "T1117", "T1170", "T1191", "T1085", "T1070", "T1220", "T1055", "T1064", "T1140", "T1197", "T1121", "T1118" };
            string[] credential_access = new string[] { "T1110", "T1208", "T1003" };
            string[] discovery = new string[] { "T1135", "T1046", "T1087", "T1007", "T1033", "T1049", "T1016", "T1083" };
            string[] lateral_movement = new string[] { "T1021", "T1028", "T1047" };

            string[] supported_techniques = execution.Union(persistence).Union(privelege_escalation).Union(defense_evasion).Union(credential_access).Union(discovery).Union(lateral_movement).ToArray();


            if (args.Length == 0)
            {
                Usage();
                return;

            }

            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    switch (args[i])
                    {
                        case "/pb":
                            pb_file = args[i + 1];
                            break;
                        case "/rhost":
                            rhost = args[i + 1];
                            remote = true;
                            break;
                        case "/ruser":
                            ruser = args[i + 1];
                            break;
                        case "/d":
                            domain = args[i + 1];
                            break;
                        case "/rpwd":
                            rpwd = args[i + 1];
                            break;
                        case "/dc":
                            dc = args[i + 1];
                            break;
                        case "/t":
                            techniques = args[i + 1];
                            break;
                        case "/scoutpath":
                            scoutfpath = args[i + 1];
                            break;
                        case "/simpath":
                            simrpath = args[i + 1];
                            break;
                        case "/users":
                            usertype = Int32.Parse(args[i + 1]);
                            break;
                        case "/hosts":
                            hosttype = Int32.Parse(args[i + 1]);
                            break;
                        case "/prot":
                            protocol = Int32.Parse(args[i + 1]);
                            break;
                        case "/pbsleep":
                            pbsleep = Int32.Parse(args[i + 1]);
                            break;
                        case "/tsleep":
                            tsleep = Int32.Parse(args[i + 1]);
                            break;
                        case "/type":
                            type = Int32.Parse(args[i + 1]);
                            break;
                        case "/opsec":
                            opsec = true;
                            break;
                        case "/v":
                            verbose = true;
                            break;
                        case "/o":
                            scoutservice = true;
                            break;
                        case "/s":
                            simservice = true;
                            break;
                        case "/n":
                            newchild = true;
                            break;
                        case "/scout":
                            scout = true;
                            scout_action = args[i + 1];
                            break;
                        case "/navigator":
                            navigator = true;
                            nav_action = args[i + 1];
                            if (nav_action.Equals("import")) navfile = args[i + 2];
                            break;
                        default:
                            break;
                    }

                }
                catch
                {
                    Console.WriteLine("[*] Error parsing parameters :( ");
                    Console.WriteLine("[*] Exiting");
                    return;
                }

            }
            if (newchild)
            {
                const uint NORMAL_PRIORITY_CLASS = 0x0020;
                Structs.PROCESS_INFORMATION pInfo = new Structs.PROCESS_INFORMATION();
                Structs.STARTUPINFO sInfo = new Structs.STARTUPINFO();
                Structs.SECURITY_ATTRIBUTES pSec = new Structs.SECURITY_ATTRIBUTES();
                Structs.SECURITY_ATTRIBUTES tSec = new Structs.SECURITY_ATTRIBUTES();
                pSec.nLength = Marshal.SizeOf(pSec);
                tSec.nLength = Marshal.SizeOf(tSec);
                string currentbin = System.Reflection.Assembly.GetEntryAssembly().Location;
                WinAPI.CreateProcess(null, currentbin + " /s", ref pSec, ref tSec, false, NORMAL_PRIORITY_CLASS, IntPtr.Zero, null, ref sInfo, out pInfo);
                return;
            }
            if (scoutservice)
            {
                NamedPipes.RunScoutService("testpipe", log);
                return;
            }
            if (simservice)
            {
                string[] options = NamedPipes.RunSimulationService("simargs", log);
                ExecuteTechniques(options[0], type, usertype, nusers, hosttype, nhosts, protocol, Int32.Parse(options[1]), Int32.Parse(options[2]), pwd, command, log, cleanup);
                return;
            }

            if (navigator)
            {

                if (nav_action.Equals("export"))
                {
                    try
                    {
                        Console.WriteLine("[+] PurpleSharp supports "+ supported_techniques.Count() +" unique ATT&CK techniques.");
                        Console.WriteLine("[+] Generating an ATT&CK Navigator layer...");
                        Json.ExportAttackLayer(supported_techniques.Distinct().ToArray());
                        Console.WriteLine("[!] Open PurpleSharp.json on https://mitre-attack.github.io/attack-navigator");
                        return;
                    }
                    catch
                    {
                        Console.WriteLine("[!] Error generating JSON layer...");
                        Console.WriteLine("[!] Exitting...");
                        return;
                    }
                }
                else if (nav_action.Equals("import"))
                {
                    Console.WriteLine("[+] Loading {0}", navfile);
                    string json = File.ReadAllText(navfile);
                    NavigatorLayer layer = Json.ReadNavigatorLayer(json);
                    Console.WriteLine("[!] Loaded attack navigator '{0}'", layer.name);
                    Console.WriteLine("[+] Converting ATT&CK navigator Json...");
                    SimulationExercise engagement = Json.ConvertNavigatorToSimulationExercise(layer, supported_techniques.Distinct().ToArray());
                    Json.CreateSimulationExercise(engagement);
                    Console.WriteLine("[!] Done");
                    Console.WriteLine("[+] Open simulation.json");
                    return;
                }
                else
                {
                    Console.WriteLine("[!] Didnt recognize parameter...");
                    Console.WriteLine("[!] Exitting...");
                    return;
                }

            }
            if (scout && !scout_action.Equals(""))
            {
                if (!rhost.Equals("") && !domain.Equals("") && !ruser.Equals(""))
                {
                    if (rpwd == "")
                    {
                        Console.Write("Password for {0}\\{1}: ", domain, ruser);
                        rpwd = Utils.GetPassword();
                        Console.WriteLine();
                    }

                    if (!rhost.Equals("random"))
                    {
                        PreAssessment(rhost, domain, ruser, rpwd, scoutfpath, log, scout_action, verbose);
                        return;
                    }
                    else if (!dc.Equals(""))
                    {
                        List<Computer> targets = new List<Computer>();
                        targets = Ldap.GetADComputers(10, dc, ruser, rpwd);
                        if (targets.Count > 0)
                        {
                            Console.WriteLine("[+] Obtained {0} possible targets.", targets.Count);
                            var random = new Random();
                            int index = random.Next(targets.Count);
                            Console.WriteLine("[+] Picked Random host for simulation: " + targets[index].Fqdn);
                            PreAssessment(targets[index].ComputerName, domain, ruser, rpwd, scoutfpath, log, scout_action, verbose);
                            return;
                        }
                        else
                        {
                            Console.WriteLine("[!] Could not obtain targets for the simulation");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("[*] Missing parameters :( ");
                        Console.WriteLine("[*] Exiting");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("[*] Missing parameters :( ");
                    Console.WriteLine("[*] Exiting");
                    return;
                }
            }
            if (!pb_file.Equals(""))
            {
                string json = File.ReadAllText(pb_file);
                SimulationExercise engagement = Json.ReadSimulationPlaybook(json);

                if (engagement != null)
                {
                    Console.Write("Submit Password for {0}\\{1}: ", engagement.domain, engagement.username);
                    string pass = Utils.GetPassword();
                    Console.WriteLine("[+] PurpleSharp will execute {0} playbook(s)", engagement.playbooks.Count);

                    SimulationExerciseResult engagementResults = new SimulationExerciseResult();
                    engagementResults.playbookresults = new List<SimulationPlaybookResult>();

                    SimulationPlaybook lastPlaybook = engagement.playbooks.Last();
                    foreach (SimulationPlaybook playbook in engagement.playbooks)
                    {
                        SimulationPlaybookResult playbookResults = new SimulationPlaybookResult();
                        playbookResults.taskresults = new List<PlaybookTaskResult>();
                        playbookResults.name = playbook.name;
                        playbookResults.host = playbook.host;
                        Console.WriteLine("[+] Starting Execution of {0}", playbook.name);

                        PlaybookTask lastTask = playbook.tasks.Last();
                        List<string> techs = new List<string>();
                        foreach (PlaybookTask task in playbook.tasks)
                        {
                            techs.Add(task.technique);
                        }
                        string techs2 = String.Join(",", techs);
                        //PlaybookTaskResult taskResult = new PlaybookTaskResult();

                        Console.WriteLine("[+] Executing techniques {0} against {1}", techs2, playbook.host);

                        if (playbook.host.Equals("random"))
                        {
                            List<Computer> targets = Ldap.GetADComputers(10, engagement.dc, engagement.username, pass);
                            if (targets.Count > 0)
                            {
                                Console.WriteLine("[+] Obtained {0} possible targets.", targets.Count);
                                var random = new Random();
                                int index = random.Next(targets.Count);
                                Console.WriteLine("[+] Picked random host for simulation: " + targets[index].Fqdn);
                                playbookResults = ExecuteRemoteTechniqueJson(targets[index].Fqdn, engagement.domain, engagement.username, pass, techs2, playbook.sleep, playbook.scoutfpath, playbook.simrpath, log, true, false);
                                playbookResults.name = playbook.name;
                                //taskResult = ExecuteRemoteTechniqueJson(targets[index].Fqdn, engagement.domain, engagement.username, pass, techs2, playbook.scoutfpath, playbook.simrpath, log, true, false);
                                //playbookResults.taskresults.Add(taskResult);
                                //if (playbook.sleep > 0 && !task.Equals(lastTask))

                                /*
                                if (playbook.sleep > 0 )
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("[+] Sleeping {0} minutes until next playbook...", playbook.sleep);
                                    System.Threading.Thread.Sleep(1000 * playbook.sleep);
                                }
                                */
                            }
                            else Console.WriteLine("[!] Could not obtain targets for the simulation");

                        }
                        else
                        {
                            playbookResults = ExecuteRemoteTechniqueJson(playbook.host, engagement.domain, engagement.username, pass, techs2, playbook.sleep, playbook.scoutfpath, playbook.simrpath, log, true, false);
                            playbookResults.name = playbook.name;
                            //taskResult = ExecuteRemoteTechniqueJson(playbook.host, engagement.domain, engagement.username, pass, techs2, playbook.scoutfpath, playbook.simrpath, log, true, false);
                            //playbookResults.taskresults.Add(taskResult);
                            if (playbook.sleep > 0)
                            //if (playbook.sleep > 0 && !task.Equals(lastTask))
                            {
                                Console.WriteLine();
                                Console.WriteLine("[+] Sleeping {0} minutes until next task...", playbook.sleep);
                                System.Threading.Thread.Sleep(1000 * playbook.sleep);
                            }
                        }
                        if (engagement.sleep > 0 && !playbook.Equals(lastPlaybook))
                        {
                            Console.WriteLine();
                            Console.WriteLine("[+] Sleeping {0} minutes until next playbook...", engagement.sleep);
                            System.Threading.Thread.Sleep(1000 * playbook.sleep);
                        }
                        
                        engagementResults.playbookresults.Add(playbookResults);
                    }

                    Console.WriteLine("Writting JSON results...");
                    Json.WriteJsonPlaybookResults(engagementResults);
                    Console.WriteLine("DONE. Open results.json");
                    return;

                }
                else Console.WriteLine("[!] Could not parse JSON input.");
                return;
            }
            if (remote)
            {
                if (!rhost.Equals("") && !domain.Equals("") && !ruser.Equals("") && !techniques.Equals(""))
                {
                    if (rpwd == "")
                    {
                        Console.Write("Password for {0}\\{1}: ", domain, ruser);
                        rpwd = Utils.GetPassword();
                        Console.WriteLine();
                    }
                    if (!rhost.Equals("random"))
                    {
                        ExecuteRemoteTechniques(rhost, domain, ruser, rpwd, techniques, pbsleep, tsleep, scoutfpath, simrpath, log, opsec, verbose);
                        return;
                    }
                    else if (!dc.Equals(""))
                    {
                        List<Computer> targets = new List<Computer>();
                        targets = Ldap.GetADComputers(10, dc, ruser, rpwd);
                        if (targets.Count > 0)
                        {
                            Console.WriteLine("[+] Obtained {0} possible targets.", targets.Count);
                            var random = new Random();
                            int index = random.Next(targets.Count);
                            Console.WriteLine("[+] Picked Random host for simulation: " + targets[index].Fqdn);
                            ExecuteRemoteTechniques(targets[index].Fqdn, domain, ruser, rpwd, techniques, pbsleep, tsleep, scoutfpath, simrpath, log, opsec, verbose);
                            return;
                        }
                        else
                        {
                            Console.WriteLine("[!] Could not obtain targets for the simulation");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("[*] Missing dc :( ");
                        Console.WriteLine("[*] Exiting");
                        return;
                    }

                }
                else
                {
                    Console.WriteLine("[*] Missing parameters :( ");
                    Console.WriteLine("[*] Exiting");
                    return;
                }
            }
            else
            {
                ExecuteTechniques(techniques, type, usertype, nusers, hosttype, nhosts, protocol, pbsleep, tsleep, pwd, command, log, cleanup);
            }

        }

        public static void PreAssessment(string rhost, string domain, string ruser, string rpwd, string scoutfpath, string log, string scout_action, bool verbose)
        {
            List<String> actions = new List<string>() { "all", "wef", "pws", "ps", "svcs", "auditpol", "cmdline" };

            if (!actions.Contains(scout_action))
            {
                Console.WriteLine("[*] Not supported.");
                Console.WriteLine("[*] Exiting");
                return;
            }

            if (rpwd == "")
            {
                Console.Write("Password for {0}\\{1}: ", domain, ruser);
                rpwd = Utils.GetPassword();
                Console.WriteLine();
            }

            string uploadPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            int index = scoutfpath.LastIndexOf(@"\");
            string scoutFolder = scoutfpath.Substring(0, index + 1);
            string args = "/o";

            Console.WriteLine("[+] Uploading Scout agent to {0} on {1}", scoutfpath, rhost);
            RemoteLauncher.upload(uploadPath, scoutfpath, rhost, ruser, rpwd, domain);

            Console.WriteLine("[+] Executing the Scout agent via WMI ...");
            RemoteLauncher.wmiexec(rhost, scoutfpath, args, domain, ruser, rpwd);
            Console.WriteLine("[+] Connecting to the Scout agent ...");

            string result = NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "SYN");
            if (result.Equals("SYN/ACK"))
            {
                Console.WriteLine("[+] OK");
                string results;

                if (scout_action.Equals("all"))
                {
                    string temp;

                    temp = NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "wef");
                    results = Encoding.UTF8.GetString(Convert.FromBase64String(temp));

                    temp = NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "pws");
                    results += Encoding.UTF8.GetString(Convert.FromBase64String(temp));

                    temp = NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "cmdline");
                    results += Encoding.UTF8.GetString(Convert.FromBase64String(temp));

                    temp = NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "ps");
                    results += Encoding.UTF8.GetString(Convert.FromBase64String(temp));

                    temp = NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "svcs");
                    results += Encoding.UTF8.GetString(Convert.FromBase64String(temp));

                    temp = NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "auditpol");
                    results += Encoding.UTF8.GetString(Convert.FromBase64String(temp));

                    NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "quit");

                }
                else 
                {
                    results = NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", scout_action);
                    results = Encoding.UTF8.GetString(Convert.FromBase64String(results));
                    NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "quit");
                }
                if (verbose)
                {
                    Console.WriteLine("[+] Grabbing the Scout Agent output...");
                    System.Threading.Thread.Sleep(1000);
                    string sresults = RemoteLauncher.readFile(rhost, scoutFolder + log, ruser, rpwd, domain);
                    Console.WriteLine("[+] Results:");
                    Console.WriteLine();
                    Console.WriteLine(sresults);
                }
                Console.WriteLine("[+] Scout Results...");
                Console.WriteLine();
                Console.WriteLine(results);
                Console.WriteLine();
                Console.WriteLine("[+] Cleaning up...");
                Console.WriteLine("[+] Deleting " + @"\\" + rhost + @"\" + scoutfpath.Replace(":", "$"));
                RemoteLauncher.delete(scoutfpath, rhost, ruser, rpwd, domain);
                Console.WriteLine("[+] Deleting " + @"\\" + rhost + @"\" + (scoutFolder + log).Replace(":", "$"));
                RemoteLauncher.delete(scoutFolder + log, rhost, ruser, rpwd, domain);
            }
        }

        public static void ExecuteRemoteTechniques(string rhost, string domain, string ruser, string rpwd, string techniques, int pbsleep, int tsleep, string scoutfpath, string simrpath, string log, bool opsec, bool verbose)
        {
            // techniques that need to be executed from a high integrity process
            string[] privileged_techniques = new string[] { "T1003", "T1136", "T1070", "T1050", "T1084" };

            if (rpwd == "")
            {
                Console.Write("Password for {0}\\{1}: ", domain, ruser);
                rpwd = Utils.GetPassword();
                Console.WriteLine();
            }

            string uploadPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            int index = scoutfpath.LastIndexOf(@"\");
            string scoutFolder = scoutfpath.Substring(0, index + 1);

            System.Threading.Thread.Sleep(3000);

            if (opsec)
            {
                string result = "";
                string args = "/o";

                Console.WriteLine("[+] Uploading Scout agent to {0} on {1}", scoutfpath, rhost);
                RemoteLauncher.upload(uploadPath, scoutfpath, rhost, ruser, rpwd, domain);

                Console.WriteLine("[+] Executing the Scout agent via WMI ...");
                RemoteLauncher.wmiexec(rhost, scoutfpath, args, domain, ruser, rpwd);
                Console.WriteLine("[+] Connecting to the Scout agent ...");

                result = NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "SYN");
                if (result.Equals("SYN/ACK"))
                {
                    Console.WriteLine("[+] OK");

                    if (privileged_techniques.Contains(techniques.ToUpper())) result = NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "recon:privileged");
                    else result = NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "recon:regular");

                    string[] payload = result.Split(',');
                    string duser = payload[0];


                    if (duser == "")
                    {
                        Console.WriteLine("[!] Could not identify a suitable process for the simulation. Is a user logged in on: " + rhost + "?");
                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "quit");
                        Thread.Sleep(1000);
                        RemoteLauncher.delete(scoutfpath, rhost, ruser, rpwd, domain);
                        RemoteLauncher.delete(scoutFolder + log, rhost, ruser, rpwd, domain);
                        Console.WriteLine("[!] Exitting.");
                        return;
                    }
                    else
                    {
                        string user = duser.Split('\\')[1];
                        //Console.WriteLine("[+] Sending simulator binary...");
                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "simrpath:" + simrpath);
                        //Console.WriteLine("[+] Sending technique ...");
                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "technique:" + techniques);
                        //Console.WriteLine("[+] Sending opsec techqniue...");
                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "opsec:" + "ppid");
                        //Console.WriteLine("[+] Sending sleep...");
                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "pbsleep:" + pbsleep.ToString());

                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "tsleep:" + tsleep.ToString());


                        Console.WriteLine("[!] Recon -> " + String.Format("Logged user: {0} | Process: {1}.exe | PID: {2} | High Integrity: {3}", duser, payload[1], payload[2], payload[3]));
                        string simfpath = "C:\\Users\\" + user + "\\" + simrpath;
                        int index2 = simrpath.LastIndexOf(@"\");
                        string simrfolder = simrpath.Substring(0, index2 + 1);

                        string simfolder = "C:\\Users\\" + user + "\\" + simrfolder;

                        Console.WriteLine("[+] Uploading Simulation agent to " + simfpath);
                        RemoteLauncher.upload(uploadPath, simfpath, rhost, ruser, rpwd, domain);

                        Console.WriteLine("[+] Triggering simulation...");
                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "act");
                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "quit");

                        //System.Threading.Thread.Sleep(5000);
                        //Console.WriteLine("[+] Sending technique to simulation agent...");
                        //Lib.NamedPipes.RunClient(rhost, domain, ruser, rpwd, "simargs", "technique:"+technique);

                        if (verbose)
                        {
                            Console.WriteLine("[+] Grabbing the Scout Agent output...");
                            System.Threading.Thread.Sleep(1000);
                            string sresults = RemoteLauncher.readFile(rhost, scoutFolder + log, ruser, rpwd, domain);
                            Console.WriteLine("[+] Results:");
                            Console.WriteLine();
                            Console.WriteLine(sresults);
                        }
                        Thread.Sleep(5000);
                        bool finished = false;
                        int counter = 1;
                        string results = RemoteLauncher.readFile(rhost, simfolder + log, ruser, rpwd, domain);
                        while (finished == false)
                        {
                            
                            if (results.Split('\n').Last().Contains("Playbook Finished"))
                            {
                                //Console.WriteLine("[+] Obtaining the Simulation Agent output...");
                                Console.WriteLine("[+] Results:");
                                Console.WriteLine();
                                Console.WriteLine(results);
                                Console.WriteLine();
                                Console.WriteLine("[+] Cleaning up...");
                                Console.WriteLine("[+] Deleting " + @"\\" + rhost + @"\" + scoutfpath.Replace(":", "$"));
                                RemoteLauncher.delete(scoutfpath, rhost, ruser, rpwd, domain);
                                Console.WriteLine("[+] Deleting " + @"\\" + rhost + @"\" + (scoutFolder + log).Replace(":", "$"));
                                RemoteLauncher.delete(scoutFolder + log, rhost, ruser, rpwd, domain);
                                Console.WriteLine("[+] Deleting " + @"\\" + rhost + @"\" + simfpath.Replace(":", "$"));
                                RemoteLauncher.delete(simfpath, rhost, ruser, rpwd, domain);
                                Console.WriteLine("[+] Deleting " + @"\\" + rhost + @"\" + (simfolder + log).Replace(":", "$"));
                                RemoteLauncher.delete(simfolder + log, rhost, ruser, rpwd, domain);
                                finished = true;
                            }
                            else
                            {
                                Console.WriteLine("[+] Not finished. Waiting an extra {0} seconds", counter * 10);
                                Thread.Sleep(counter * 10 * 1000);
                                results = RemoteLauncher.readFile(rhost, simfolder + log, ruser, rpwd, domain);
                            }
                            counter += 1;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("[!] Could not connect to namedpipe service");
                    Console.WriteLine("[!] Exitting.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("[+] Uploading Scout Agent to {0} on {1}", scoutfpath, rhost);
                RemoteLauncher.upload(uploadPath, scoutfpath, rhost, ruser, rpwd, domain);
                string cmdline = "/s";

                Console.WriteLine("[+] Executing Scout Agent via WMI ...");
                RemoteLauncher.wmiexec(rhost, scoutfpath, cmdline, domain, ruser, rpwd);

                Thread.Sleep(2000);
                NamedPipes.RunClient(rhost, domain, ruser, rpwd, "simargs", "technique:" + techniques + " pbsleep:" + pbsleep.ToString() + " tsleep:" + tsleep.ToString());

                Thread.Sleep(5000);
                bool finished = false;
                int counter = 1;
                string results = RemoteLauncher.readFile(rhost, scoutFolder + log, ruser, rpwd, domain);
                while (finished == false)
                {
                    
                    if (results.Split('\n').Last().Contains("Playbook Finished"))
                    {
                        Console.WriteLine("[+] Obtaining results...");
                        Console.WriteLine("[+] Results:");
                        Console.WriteLine();
                        Console.WriteLine(results);
                        Console.WriteLine();
                        Console.WriteLine("[+] Cleaning up...");
                        Console.WriteLine("[+] Deleting " + @"\\" + rhost + @"\" + scoutfpath.Replace(":", "$"));
                        RemoteLauncher.delete(scoutfpath, rhost, ruser, rpwd, domain);
                        Console.WriteLine("[+] Deleting " + @"\\" + rhost + @"\" + (scoutFolder + log).Replace(":", "$"));
                        RemoteLauncher.delete(scoutFolder + log, rhost, ruser, rpwd, domain);
                        finished = true;
                    }
                    else
                    {
                        Console.WriteLine("[+] Not finished. Waiting an extra {0} seconds", counter * 10);
                        Thread.Sleep(counter * 10 * 1000);
                        results = RemoteLauncher.readFile(rhost, scoutFolder + log, ruser, rpwd, domain);
                    }
                    counter += 1;
                }
            }
        }
        public static SimulationPlaybookResult ExecuteRemoteTechniqueJson(string rhost, string domain, string ruser, string rpwd, string technique, int sleep, string scoutfpath, string simrpath, string log, bool opsec, bool verbose)
        {
            // techniques that need to be executed from a high integrity process
            string[] privileged_techniques = new string[] { "T1003", "T1136", "T1070", "T1050", "T1084" };

            string uploadPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            int index = scoutfpath.LastIndexOf(@"\");
            string scoutFolder = scoutfpath.Substring(0, index + 1);
            Thread.Sleep(3000);

            if (opsec)
            {
                string result = "";
                string args = "/o";

                //Console.WriteLine("[+] Uploading Scout agent to {0} on {1}", scoutfpath, rhost);
                RemoteLauncher.upload(uploadPath, scoutfpath, rhost, ruser, rpwd, domain);

                //Console.WriteLine("[+] Executing the Scout agent via WMI ...");
                RemoteLauncher.wmiexec(rhost, scoutfpath, args, domain, ruser, rpwd);
                //Console.WriteLine("[+] Connecting to namedpipe service ...");

                result = NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "SYN");
                if (result.Equals("SYN/ACK"))
                {
                    //Console.WriteLine("[+] OK");

                    if (privileged_techniques.Contains(technique.ToUpper())) result = NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "recon:privileged");
                    else result = NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "recon:regular");

                    string[] payload = result.Split(',');
                    string duser = payload[0];


                    if (duser == "")
                    {
                        Console.WriteLine("[!] Could not identify a suitable process for the simulation. Is a user logged in on: " + rhost + "?");
                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "quit");
                        Thread.Sleep(1000);
                        RemoteLauncher.delete(scoutfpath, rhost, ruser, rpwd, domain);
                        RemoteLauncher.delete(scoutFolder + log, rhost, ruser, rpwd, domain);
                        //Console.WriteLine("[!] Exitting.");
                        return null;
                    }
                    else
                    {
                        string user = duser.Split('\\')[1];
                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "simrpath:" + simrpath);
                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "technique:" + technique);
                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "opsec:" + "ppid");
                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "sleep:" + sleep.ToString());

                        string simfpath = "C:\\Users\\" + user + "\\" + simrpath;
                        int index2 = simrpath.LastIndexOf(@"\");
                        string simrfolder = simrpath.Substring(0, index2 + 1);

                        string simfolder = "C:\\Users\\" + user + "\\" + simrfolder;

                        //Console.WriteLine("[+] Uploading Simulation agent to " + simfpath);
                        RemoteLauncher.upload(uploadPath, simfpath, rhost, ruser, rpwd, domain);

                        //Console.WriteLine("[+] Triggering simulation...");
                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "act");
                        NamedPipes.RunClient(rhost, domain, ruser, rpwd, "testpipe", "quit");

                        System.Threading.Thread.Sleep(5000);
                        bool finished = false;
                        int counter = 1;
                        string results = RemoteLauncher.readFile(rhost, simfolder + log, ruser, rpwd, domain);
                        while (finished == false)
                        {
                            if (results.Split('\n').Last().Contains("Playbook Finished"))
                            {
                                Console.WriteLine("[+] Results:");
                                Console.WriteLine();
                                Console.WriteLine(results);
                                RemoteLauncher.delete(scoutfpath, rhost, ruser, rpwd, domain);
                                RemoteLauncher.delete(scoutFolder + log, rhost, ruser, rpwd, domain);
                                RemoteLauncher.delete(simfpath, rhost, ruser, rpwd, domain);
                                RemoteLauncher.delete(simfolder + log, rhost, ruser, rpwd, domain);
                                finished = true;
                                
                                
                            }
                            else
                            {
                                Console.WriteLine("[+] Not finished. Waiting an extra {0} seconds", counter * 10);
                                Thread.Sleep(counter * 10 * 1000);
                                results = RemoteLauncher.readFile(rhost, simfolder + log, ruser, rpwd, domain);
                            }
                            
                            counter += 1;
                        }
                        return Json.GetPlaybookResult(results);

                    }
                }
                else
                {
                    //Console.WriteLine("[!] Could not connect to namedpipe service");
                    return null;
                }
            }
            else
            {
                //Console.WriteLine("[+] Uploading PurpleSharp to {0} on {1}", scoutfpath, rhost);
                RemoteLauncher.upload(uploadPath, scoutfpath, rhost, ruser, rpwd, domain);

                string cmdline = "/t " + technique;
                //Console.WriteLine("[+] Executing PurpleSharp via WMI ...");
                RemoteLauncher.wmiexec(rhost, scoutfpath, cmdline, domain, ruser, rpwd);
                Thread.Sleep(3000);
                Console.WriteLine("[+] Obtaining results...");
                string results = RemoteLauncher.readFile(rhost, scoutFolder + log, ruser, rpwd, domain);
                Console.WriteLine("[+] Results:");
                Console.WriteLine();
                Console.WriteLine(results);
                //Console.WriteLine("[+] Cleaning up...");
                //Console.WriteLine("[+] Deleting " + @"\\" + rhost + @"\" + scoutfpath.Replace(":", "$"));
                RemoteLauncher.delete(scoutfpath, rhost, ruser, rpwd, domain);
                //
                //Console.WriteLine("[+] Deleting " + @"\\" + rhost + @"\" + (scoutFolder + log).Replace(":", "$"));
                RemoteLauncher.delete(scoutFolder + log, rhost, ruser, rpwd, domain);

                return Json.GetPlaybookResult(results);
            }
        }
        public static void ExecuteTechnique(string technique, int type, int usertype, int nuser, int computertype, int nhosts, int protocol, int tsleep, string password, string command, string log, bool cleanup)
        {
            switch (technique)
            {

                // Initial Access

                // Execution

                case "T1117":
                    Simulations.Execution.ExecuteRegsvr32(log);
                    break;

                case "T1064":
                    Simulations.Execution.Scripting(log);
                    break;

                case "T1035":
                    Simulations.Execution.ServiceExecution(log);
                    break;

                //T1053 - Scheduled Task
                case "T1053":
                    Simulations.LateralMovement.CreateSchTaskOnHosts(computertype, nhosts, tsleep, command, cleanup);
                    break;

                case "T1059":
                    Simulations.Execution.ExecuteCmd(log);
                    break;

                case "T1086":
                    Simulations.Execution.ExecutePowershell(log);
                    break;

                //T1028 - Windows Remote Management

                // Persistence

                //T1053 - Scheduled Task

                case "T1136":
                    //Simulations.Persistence.CreateAccountCmd(log);
                    Simulations.Persistence.CreateAccountApi(log);
                    break;

                case "T1050":
                    Simulations.Persistence.CreateServiceApi(log);
                    //Simulations.Persistence.CreateServiceCmd(log);
                    break;

                case "T1060":
                    Simulations.Persistence.RegistryRunKeyNET(log);
                    //Simulations.Persistence.RegistryRunKeyCmd(log);
                    break;

                case "T1084":
                    Simulations.Persistence.WMIEventSubscription(log);
                    //Simulations.Persistence.RegistryRunKeyCmd(log);
                    break;

                // Privilege Escalation

                //T1050 - New Service

                //T1053 - Scheduled Task

                // Defense Evasion

                case "T1121":
                    Simulations.DefenseEvasion.RegsvcsRegasm(log);
                    break;

                case "T1118":
                    Simulations.DefenseEvasion.InstallUtil(log);
                    break;

                case "T1140":
                    Simulations.DefenseEvasion.DeobfuscateDecode(log);
                    break;

                case "T1170":
                    Simulations.DefenseEvasion.Mshta(log);
                    break;

                case "T1191":
                    Simulations.DefenseEvasion.Csmtp(log);
                    break;

                case "T1197":
                    Simulations.DefenseEvasion.BitsJobs(log);
                    break;


                case "T1085":
                    Simulations.DefenseEvasion.Rundll32(log);
                    break;

                case "T1070":
                    //Simulations.DefenseEvasion.ClearSecurityEventLogCmd(log);
                    Simulations.DefenseEvasion.ClearSecurityEventLogNET(log);
                    break;

                case "T1220":
                    Simulations.DefenseEvasion.XlScriptProcessing(log);
                    break;

                case "T1055":
                    Simulations.DefenseEvasion.ProcessInjection(log);
                    break;

                //T1117 - Regsvr32


                // Credential Access

                //T1110 - Brute Force
                case "T1110":
                    if (type == 1)
                    {
                        Simulations.CredAccess.LocalDomainPasswordSpray(usertype, nuser, protocol, tsleep, password, log); ;
                        break;
                    }
                    else if (type == 3)
                    {
                        Simulations.CredAccess.RemotePasswordSpray(type, computertype, nhosts, usertype, nuser, protocol, tsleep, password, log);
                        break;
                    }
                    break;

                //T1208 - Kerberoasting
                case "T1208":
                    Simulations.CredAccess.Kerberoasting(log, tsleep);
                    break;

                //T1003 - Credential Dumping
                case "T1003":
                    Simulations.CredAccess.Lsass(log);
                    break;

                // Discovery

                //T1016 System Network Configuration Discovery
                case "T1016":
                    Simulations.Discovery.SystemNetworkConfigurationDiscovery(log);
                    break;

                //T1083 File and Directory Discovery
                case "T1083":
                    Simulations.Discovery.FileAndDirectoryDiscovery(log);
                    break;

                //T1135 - Network Share Discovery
                case "T1135":
                    Simulations.Discovery.EnumerateShares(computertype, nhosts, tsleep, log);
                    break;

                //T1046 - Network Service Scanning
                case "T1046":
                    Simulations.Discovery.NetworkServiceDiscovery(computertype, nhosts, tsleep, log);
                    break;

                case "T1087":
                    Simulations.Discovery.AccountDiscoveryLdap(log);
                    //Simulations.Discovery.AccountDiscoveryCmd(log);
                    break;

                case "T1007":
                    Simulations.Discovery.SystemServiceDiscovery(log);
                    break;

                case "T1033":
                    Simulations.Discovery.SystemUserDiscovery(log);
                    break;

                case "T1049":
                    Simulations.Discovery.SystemNetworkConnectionsDiscovery(log);
                    break;

                // Lateral Movement

                //T1028 - Windows Remote Management
                case "T1028":
                    Simulations.LateralMovement.ExecuteWinRMOnHosts(computertype, nhosts, tsleep, command, log);
                    break;

                //T1021 - Remote Service
                case "T1021":
                    Simulations.LateralMovement.CreateRemoteServiceOnHosts(computertype, nhosts, tsleep, cleanup, log);
                    break;

                //T1047 - Windows Management Instrumentation
                case "T1047":
                    Simulations.LateralMovement.ExecuteWmiOnHosts(computertype, nhosts, tsleep, command, log);
                    break;

                // Collection

                // Command and Control

                // Exfiltration

                // Impact

                // Other Techniques

                case "privenum":
                    Simulations.Discovery.PrivilegeEnumeration(computertype, nhosts, tsleep);
                    break;

                default:
                    break;

            }
        }
        public static void ExecuteTechniques(string technique, int type, int usertype, int nuser, int computertype, int nhosts, int protocol, int pbsleep, int tsleep, string password, string command, string log, bool cleanup)
        {
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            Logger logger = new Logger(currentPath + log);

            if (technique.Contains(","))
            {
                string[] techniques = technique.Split(',');
                for (int i=0; i < techniques.Length; i++)
                {
                    ExecuteTechnique(techniques[i].Trim(), type, usertype, nuser, computertype, nhosts, protocol, tsleep, password, command, log, cleanup);
                    if (pbsleep > 0 && i != techniques.Length-1) Thread.Sleep(1000 * pbsleep);
                }
                logger.TimestampInfo("Playbook Finished");
            }
            else 
            {
                ExecuteTechnique(technique, type, usertype, nuser, computertype, nhosts, protocol, tsleep, password, command, log, cleanup);
                logger.TimestampInfo("Playbook Finished");
            }
        }

    }
}