using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BSB
{
	public class BSBData
	{
		public string strDataPath = @"D:\LibNLPDB\BSB\Data\2019\";

        public SortedList<int, BSBRecord> slBSB = new SortedList<int, BSBRecord>();
        public SortedList<int, BSBRecord> slBSBForeignLanguageKey = new SortedList<int, BSBRecord>();

        public Dictionary<string, List<string>> dBSBChunksEH = new Dictionary<string, List<string>>(); //D<english, L<hebrew>>
        public Dictionary<string, List<string>> dBSBChunksEG = new Dictionary<string, List<string>>(); //D<english, L<greek>>
		public Dictionary<int, List<string>> dBSBForeignOrderedEnglish = new Dictionary<int, List<string>>(); //D<foreign ordering, List<english>>
		public Dictionary<string, List<string>> dBSBChunksHebrew = new Dictionary<string, List<string>>(); //D<hebrew, L<english>>
		public Dictionary<string, List<string>> dBSBChunksGreek = new Dictionary<string, List<string>>(); //D<greek, L<english>>
		public Dictionary<string, List<string>> dBSBParses = new Dictionary<string, List<string>>(); //D<english, L<
        public Dictionary<string, List<SemanticTag>> dBSBSemanticTags = new Dictionary<string, List<SemanticTag>>();
		public Dictionary<string, List<string>> dSemanticParts = new Dictionary<string, List<string>> ();
		public Dictionary<string, List<string>> dSemanticPartsReversed = new Dictionary<string, List<string>> ();
		public Dictionary<string, string> dBasicWords = new Dictionary<string, string>(); //D<complex, basic> ie D["tested"] = "test", D["testing"] = test, D["test"] = "trial", D["trial"] = "test", D["test"] != "testing", NOTE: the pair of example with "trial" and "test" are back and forth because they are ambiguous in their generality ordering
        public Dictionary<string, Dictionary<string, int>> dGreekPartEnglishCounts = new Dictionary<string, Dictionary<string, int>>();
        Dictionary<string, Dictionary<int, List<string>>> dOrderedGPEC = new Dictionary<string, Dictionary<int, List<string>>>(); //D<greek part(s), D<count, L<english word>>>
		public BSBData ()
        {
            StreamWriter swReconstructedEnglishText = null;
            StringBuilder sbEnglish = null;

            //ReadBSBInterlinear()
            ReadBSBInterlinear();
            GreekParts();

            //Optional File Writing
            if (1 == 1)
            {
                WriteDetailedParseFiles();
            }

            //MakeEnglish()
            sbEnglish = MakeEnglish();

            //Write Reconstructed English File
            swReconstructedEnglishText = new StreamWriter(strDataPath + @"BSB-ReconstructedEnglish.text");
            swReconstructedEnglishText.WriteLine(sbEnglish.ToString());
            swReconstructedEnglishText.Close();

            CreateEnglishOrderedByHebrew();
        }

        public void WriteDetailedParseFiles()
        {
            Dictionary<string, Dictionary<string, int>> dParses = 
                new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, Dictionary<string, int>> dParsesForeign = 
                new Dictionary<string, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, string>> dEnglishByCount =
                new Dictionary<int, Dictionary<string, string>>();
            Dictionary<int, Dictionary<string, string>> dForeignByCount =
                new Dictionary<int, Dictionary<string, string>>();
            Dictionary<string, Dictionary<int, string>> dEnglishByTag =
                new Dictionary<string, Dictionary<int, string>>();
            Dictionary<string, Dictionary<int, string>> dForeignByTag =
                new Dictionary<string, Dictionary<int, string>>();

            StreamWriter swEnglishByTag = new StreamWriter(strDataPath + @"BSB-EnglishByTags.text");
            StreamWriter swForeignByTag = new StreamWriter(strDataPath + @"BSB-ForeignByTags.text");
            StreamWriter swEnglishByCount = new StreamWriter(strDataPath + @"BSB-EnglishByCounts.text");
            StreamWriter swForeignByCount = new StreamWriter(strDataPath + @"BSB-ForeignByCounts.text");
            StreamWriter swEnglish = new StreamWriter(strDataPath + @"BSB-English.text");
            StreamWriter swForeign = new StreamWriter(strDataPath + @"BSB-Foreign.text");

            Regex rgxOnlyAlphanum = new Regex(@"[a-z0-9 ]");

            foreach (int intKey in slBSB.Keys)
            {
                BSBRecord rec = slBSB[intKey];
                string strEnglish = "";

                foreach (Match m in rgxOnlyAlphanum.Matches(rec.strBSBVersion.Trim().ToLower()))
                {
                    strEnglish += m.Value;
                }

                strEnglish = strEnglish.Trim();

                if (!dParses.ContainsKey(strEnglish))
                {
                    dParses.Add(strEnglish, new Dictionary<string, int>());
                }

                if (rec.strParsing.Contains("|"))
                {
                    foreach (string strParsePart in rec.strParsing.Split("|".ToCharArray()))
                    {
                        if (!dParses[strEnglish].ContainsKey(strParsePart.Trim().ToLower()))
                        {
                            dParses[strEnglish].Add(strParsePart.Trim().ToLower(), 1);
                        }
                        else
                        {
                            dParses[strEnglish][strParsePart.Trim().ToLower()]++;
                        }
                    }
                }
                else
                {
                    if (!dParses[strEnglish].ContainsKey(rec.strParsing.Trim().ToLower()))
                    {
                        dParses[strEnglish].Add(rec.strParsing.Trim().ToLower(), 1);
                    }
                    else
                    {
                        dParses[strEnglish][rec.strParsing.Trim().ToLower()]++;
                    }
                }

                //foreign

                if (!dParsesForeign.ContainsKey(rec.strTransliteration.Trim().ToLower()))
                {
                    dParsesForeign.Add(rec.strTransliteration.Trim().ToLower(), new Dictionary<string, int>());
                }

                if (rec.strParsing.Contains("|"))
                {
                    foreach (string strParsePart in rec.strParsing.Split("|".ToCharArray()))
                    {
                        if (!dParsesForeign[rec.strTransliteration.Trim().ToLower()].ContainsKey(strParsePart.Trim().ToLower()))
                        {
                            dParsesForeign[rec.strTransliteration.Trim().ToLower()].Add(strParsePart.Trim().ToLower(), 1);
                        }
                        else
                        {
                            dParsesForeign[rec.strTransliteration.Trim().ToLower()][strParsePart.Trim().ToLower()]++;
                        }
                    }
                }
                else
                {
                    if (!dParsesForeign[rec.strTransliteration.Trim().ToLower()].ContainsKey(rec.strParsing.Trim().ToLower()))
                    {
                        dParsesForeign[rec.strTransliteration.Trim().ToLower()].Add(rec.strParsing.Trim().ToLower(), 1);
                    }
                    else
                    {
                        dParsesForeign[rec.strTransliteration.Trim().ToLower()][rec.strParsing.Trim().ToLower()]++;
                    }
                }
            }

            foreach (string strKey in dParses.Keys.OrderBy(a => a))
            {
                foreach (string strKey2 in dParses[strKey].Keys.OrderBy(b => b))
                {
                    swEnglish.WriteLine(strKey + " ^ " + strKey2 + " ^ " + dParses[strKey][strKey2]);

                    if (!dEnglishByCount.ContainsKey(dParses[strKey][strKey2]))
                    {
                        dEnglishByCount.Add(dParses[strKey][strKey2], new Dictionary<string, string>());
                    }

                    if (!dEnglishByCount[dParses[strKey][strKey2]].ContainsKey(strKey2))
                    {
                        dEnglishByCount[dParses[strKey][strKey2]].Add(strKey2, strKey);
                    }

                    if (!dEnglishByTag.ContainsKey(strKey2))
                    {
                        dEnglishByTag.Add(strKey2, new Dictionary<int, string>());
                    }

                    if (!dEnglishByTag[strKey2].ContainsKey(dParses[strKey][strKey2]))
                    {
                        dEnglishByTag[strKey2].Add(dParses[strKey][strKey2], strKey);
                    }
                }
            }

            foreach (string strKey in dParsesForeign.Keys.OrderBy(a => a))
            {
                foreach (string strKey2 in dParsesForeign[strKey].Keys.OrderBy(b => b))
                {
                    swForeign.WriteLine(strKey + " ^ " + strKey2 + " ^ " + dParsesForeign[strKey][strKey2]);

                    if (!dForeignByCount.ContainsKey(dParsesForeign[strKey][strKey2]))
                    {
                        dForeignByCount.Add(dParsesForeign[strKey][strKey2], new Dictionary<string, string>());
                    }

                    if (!dForeignByCount[dParsesForeign[strKey][strKey2]].ContainsKey(strKey2))
                    {
                        dForeignByCount[dParsesForeign[strKey][strKey2]].Add(strKey2, strKey);
                    }

                    if (!dForeignByTag.ContainsKey(strKey2))
                    {
                        dForeignByTag.Add(strKey2, new Dictionary<int, string>());
                    }

                    if (!dForeignByTag[strKey2].ContainsKey(dParsesForeign[strKey][strKey2]))
                    {
                        dForeignByTag[strKey2].Add(dParsesForeign[strKey][strKey2], strKey);
                    }
                }
            }

            foreach (int intKey in dEnglishByCount.Keys.OrderByDescending(a => a))
            {
                foreach (string strKey2 in dEnglishByCount[intKey].Keys.OrderBy(b => b))
                {
                    swEnglishByCount.WriteLine(intKey.ToString() + " ^ " + strKey2 + " ^ " + dEnglishByCount[intKey][strKey2]);
                }
            }

            foreach (int intKey in dForeignByCount.Keys.OrderByDescending(a => a))
            {
                foreach (string strKey2 in dForeignByCount[intKey].Keys.OrderBy(b => b))
                {
                    swForeignByCount.WriteLine(intKey + " ^ " + strKey2 + " ^ " + dForeignByCount[intKey][strKey2]);
                }
            }

            foreach (string strKey in dEnglishByTag.Keys.OrderBy(a => a))
            {
                foreach (int intKey2 in dEnglishByTag[strKey].Keys.OrderByDescending(b => b))
                {
                    swEnglishByTag.WriteLine(strKey.ToString() + " ^ " + intKey2 + " ^ " + dEnglishByTag[strKey][intKey2]);
                }
            }

            foreach (string strKey in dForeignByTag.Keys.OrderBy(a => a))
            {
                foreach (int intKey2 in dForeignByTag[strKey].Keys.OrderByDescending(b => b))
                {
                    swForeignByTag.WriteLine(strKey + " ^ " + intKey2 + " ^ " + dForeignByTag[strKey][intKey2]);
                }
            }

            swForeignByCount.Close();
            swEnglishByCount.Close();
            swForeign.Close();
            swEnglish.Close();
        }

		public void ReadBSBInterlinear()
		{
			if (File.Exists (strDataPath + @"bsb_tables_6-7-19.csv")) {
				//load bsbrecords
                StreamReader srBSBRecords = new StreamReader(strDataPath + @"bsb_tables_6-7-19.csv");
                
                //Data File Header Management
                BSBRecord brecTest = new BSBRecord();
                bool bHeader = false;

                brecTest.FromRecord(srBSBRecords.ReadLine());
                
                if (brecTest.strBSBVersion == "BSB Version")
                {
                    bHeader = true;
                }

                srBSBRecords.Close();
                srBSBRecords = new StreamReader(strDataPath + @"bsb_tables_6-7-19.csv");

                if (bHeader)
                {
                    srBSBRecords.ReadLine();
                }
                //END Header

				while (!srBSBRecords.EndOfStream) {
					BSBRecord brecAdd = new BSBRecord ();

					brecAdd.FromRecord (srBSBRecords.ReadLine ());
                    slBSB.Add(slBSB.Count() + 1, brecAdd);
				}
			}
            else
            {
				FillslBSB ();
				WriteBSBRecords (); //writes slBSB to file
			}

            foreach (int intBSort in slBSB.Keys.OrderBy(a=> a))
            {
                if (intBSort > 0)
                {
                    slBSBForeignLanguageKey.Add(intBSort, slBSB[intBSort]);
                }
            }

            if (File.Exists(strDataPath + @"bsb_tables_6-7-19-UnifiedChunks-Hebrew.txt"))
            {
				//Load Hebrew
                StreamReader srHebrew = new StreamReader(strDataPath + @"bsb_tables_6-7-19-UnifiedChunks-Hebrew.txt");

				while (!srHebrew.EndOfStream) {
					string[] strsLine = srHebrew.ReadLine ().Split('^');
					//
					if (!dBSBChunksEH.ContainsKey (strsLine [0].Trim())) {
						dBSBChunksEH.Add (strsLine [0].Trim (), new List<string> ());
					}

					dBSBChunksEH [strsLine [0].Trim()].Add (strsLine [1].Trim());
				}
			} else {
                StreamWriter swUnifiedChunksHebrew = new StreamWriter(strDataPath + @"bsb_tables_6-7-19-UnifiedChunks-Hebrew.txt");
                StreamWriter swParsing = new StreamWriter(strDataPath + @"bsb_tables_6-7-19-Parsing.txt");

				CreateHebrewOrderedByEnglish ();
				WriteParsing (ref swParsing); //Parse creation hitches a loop ride with CreateOrderingByEnglishAndHebrew
				WriteUnifiedChunksHebrew (ref swUnifiedChunksHebrew);
			}

            if (File.Exists(strDataPath + @"bsb_tables_6-7-19-UnifiedChunks-Greek.txt"))
            {
				//load Greek
                StreamReader srGreek = new StreamReader(strDataPath + @"bsb_tables_6-7-19-UnifiedChunks-Greek.txt");

				while (!srGreek.EndOfStream) {
					string[] strsLine = srGreek.ReadLine ().Split('^');
					//
					if (!dBSBChunksEG.ContainsKey (strsLine [1].Trim())) {
						dBSBChunksEG.Add (strsLine [1].Trim (), new List<string> ());
					}

					dBSBChunksEG [strsLine [1].Trim()].Add (strsLine [0].Trim());
				}
			} else {
                StreamWriter swUnifiedChunksGreek = new StreamWriter(strDataPath + @"bsb_tables_6-7-19-UnifiedChunks-Greek.txt");

				CreateGreekOrderedByEnglish ();
				WriteUnifiedChunksGreek (ref swUnifiedChunksGreek);
			}

            if (File.Exists(strDataPath + @"bsb_tables_6-7-19-SemanticParts.txt"))
            {
				//load SemanticParts
                StreamReader srSemanticParts = new StreamReader(strDataPath + @"bsb_tables_6-7-19-SemanticParts.txt");

				while (!srSemanticParts.EndOfStream) {
					string[] strsLine = srSemanticParts.ReadLine ().Split('^');

					if (!dSemanticParts.ContainsKey (strsLine [0].Trim())) {
						dSemanticParts.Add (strsLine [0].Trim (), new List<string> ());
					}

					dSemanticParts [strsLine [0].Trim()].Add (strsLine [1].Trim());

					if (!dSemanticPartsReversed.ContainsKey (strsLine [0].Trim())) {
						dSemanticPartsReversed.Add (strsLine [0].Trim (), new List<string> ());
					}

					dSemanticPartsReversed [strsLine [0].Trim()].Add (strsLine [1].Trim());
				}
			} else {
				CreateSemanticTagParts ();
				WriteSemanticTags ();
			}

			//BSBSquared (); //Experimental
		}

		public void FillslBSB(){ //Fills slBSB
			int intLineCounter = 0;
            StreamReader srBSB = new StreamReader(strDataPath + @"bsb_tables_6-7-19.csv");

			//Read in the file to slBSB
			while (!srBSB.EndOfStream) {
				string[] strsLine = srBSB.ReadLine ().Split('^');

				intLineCounter++;

				if (intLineCounter > 3 && strsLine[2].Trim() != "") { //skip first two lines in this file
					BSBRecord bsb = new BSBRecord();
                    int intHebSort, intGreekSort, intBSort = 0;
                    
					//strsLine[5] is WLC / Nestle 1904 Base {TR} ⧼RP⧽ (WH) 〈NE〉 [NA] ‹SBL›
					bsb.strHebSort = strsLine [0].Trim(); //1
					bsb.strGreekSort = strsLine [1].Trim(); //0
					bsb.strBSort = strsLine [2].Trim(); //1
					bsb.strLanguage = strsLine [3].Trim(); //Hebrew
					bsb.strVerse = strsLine [4].Trim(); //1
					bsb.strWLC = strsLine [5].Trim(); //בְּרֵאשִׁ֖ית
					bsb.strSeperator = strsLine [6].Trim();
					//bsb.strTranliteration = Regex.Replace(strsLine [7].Trim().ToLower(), 
					//	@"[;,.-·]", @""); //be·re·Shit or bereShit
					bsb.strTransliteration = strsLine [7].Trim().ToLower(); 
					bsb.strParsing = strsLine [8].Trim(); //Prep-b | N-fs
					bsb.strStrong = strsLine [9].Trim(); //7225
					bsb.strKJVVerse = strsLine [10].Trim(); //genesis 1:1
					bsb.strHeading = strsLine [11].Trim(); //The Creation
					bsb.strBSBVersion = strsLine [12].Trim().ToLower(); //in the beginning
					bsb.strFootnotes = strsLine [13].Trim(); 
					bsb.strBDBThayers = strsLine [14].Trim(); //1) first, beginning, best, chief <BR> 1a) beginning <BR> 1b) first <BR> 1c) chief <BR> 1d) choice part

                    intHebSort = Convert.ToInt32(bsb.strHebSort);
                    intGreekSort = Convert.ToInt32(bsb.strGreekSort);
                    intBSort = Convert.ToInt32(bsb.strBSort);

                    slBSB.Add (intBSort, bsb);
				}
			}

			srBSB.Close ();
		}

		public void CreateHebrewOrderedByEnglish(){
			foreach (int intBSort in slBSB.Keys){
				string strWordsOnly = Regex.Replace (slBSB [intBSort].strBSBVersion,
					@"[^A-Za-z ]", a => a.Result ("").ToString ()).Trim();
				//string strWordsOnly = slBSB[intBSort].strBSBVersion;

				if (slBSB [intBSort].strLanguage == "Hebrew") { //OT
					if (!dBSBChunksEH.Keys.Contains (strWordsOnly)) {
						dBSBChunksEH.Add (strWordsOnly, new List<string> ());
						//Console.WriteLine (strWordsOnly);
					}
					if (!dBSBChunksHebrew.Keys.Contains (slBSB [intBSort].strTransliteration)) {
						dBSBChunksHebrew.Add (slBSB [intBSort].strTransliteration, new List<string> ());
					}
					if (!dBSBChunksEH [strWordsOnly].Contains (slBSB [intBSort].strTransliteration)) {
						dBSBChunksEH [strWordsOnly].Add (slBSB [intBSort].strTransliteration);
					}
					if (!dBSBChunksHebrew [slBSB [intBSort].strTransliteration].Contains (strWordsOnly)) {
						dBSBChunksHebrew [slBSB [intBSort].strTransliteration].Add (strWordsOnly);
					}
				}

				InlineCreateParses (intBSort, strWordsOnly);
			}
		}

        public void CreateGreekOrderedByEnglish (){
			foreach (int intBSort in slBSB.Keys){
				string strWordsOnly = Regex.Replace (slBSB [intBSort].strBSBVersion,
					@"[^A-Za-z ]", a => a.Result ("").ToString ()).Trim();
				//string strWordsOnly = slBSB[intBSort].strBSBVersion;
				//string strTranslit = Regex.Replace (slBSB [intBSort].strTranliteration,
				//	@"[^A-Za-z ]", a => a.Result ("").ToString ()).Trim();
				string strTranslit = slBSB [intBSort].strTransliteration;

				if (slBSB [intBSort].strLanguage == "Greek") { //OT
					if (!dBSBChunksEG.Keys.Contains (strWordsOnly)) {
						dBSBChunksEG.Add (strWordsOnly, new List<string> ());
						//Console.WriteLine (strWordsOnly);
					}
					if (!dBSBChunksGreek.Keys.Contains (strTranslit)) {
						dBSBChunksGreek.Add (strTranslit, new List<string> ());
					}
					if (!dBSBChunksEG [strWordsOnly].Contains (strTranslit)) {
						dBSBChunksEG [strWordsOnly].Add (strTranslit);
					}
					if (!dBSBChunksGreek [strTranslit].Contains (strWordsOnly)) {
						dBSBChunksGreek [strTranslit].Add (strWordsOnly);
					}
				} 
			}
		}

        public void CreateEnglishOrderedByHebrew()
        {
            Dictionary<string, List<string>> dForeignToEnglish = new Dictionary<string, List<string>>();
            StreamWriter swBSBForeignOrderedEnglish =
                    new StreamWriter(strDataPath + @"BSBForeignOrderedEnglish.text");
            int intChunk = 0;

            foreach (int intKey in slBSB.Keys.OrderBy(a => a)){
                string strTransliteration = slBSB[intKey].strTransliteration;
                string strBSB = slBSB[intKey].strBSBVersion.ToLower();

                if (!dForeignToEnglish.ContainsKey(strTransliteration))
                {
                    dForeignToEnglish.Add(strTransliteration, new List<string>());
                }

                if (!dForeignToEnglish[strTransliteration].Contains(strBSB))
                {
                    dForeignToEnglish[strTransliteration].Add(strBSB);
                }
            }

            //Write Output in Foreign Sort Order
            //[english1 | english2 | ..] [english1 | english2 | ..]
            foreach (int intSort in slBSBForeignLanguageKey.Keys.OrderBy(a=> a))
            {
                swBSBForeignOrderedEnglish.Write("[");

                foreach (string strBSB in dForeignToEnglish[slBSBForeignLanguageKey[intSort].strTransliteration])
                {
                    swBSBForeignOrderedEnglish.Write(strBSB + " | ");
                    intChunk += strBSB.Length;

                    if (intChunk > 60)
                    {
                        intChunk = 0;
                        swBSBForeignOrderedEnglish.WriteLine();
                    }
                }

                swBSBForeignOrderedEnglish.Write("]");
                swBSBForeignOrderedEnglish.WriteLine();
                swBSBForeignOrderedEnglish.WriteLine();
            }
            
            swBSBForeignOrderedEnglish.Close();
        }

        public void InlineCreateParses(int intBSort, string strWordsOnly){
			if (!dBSBParses.Keys.Contains(slBSB [intBSort].strParsing)) {
				dBSBParses.Add (slBSB [intBSort].strParsing, new List<string> ());
			}
			if (!dBSBParses [slBSB [intBSort].strParsing].Contains (strWordsOnly)) {
				dBSBParses [slBSB [intBSort].strParsing].Add (strWordsOnly);
			}
		}

		public void CreateSemanticTagParts(){
			foreach (string strKey in dBSBParses.Keys) {
				foreach (string strValue in dBSBParses[strKey]) {
					if (strValue.ToLower () != "vvv" &&
						strValue.ToLower ().Trim() != "") {
						SemanticTag stBSB = new SemanticTag (strKey.ToLower().Trim());

						if (!dBSBSemanticTags.ContainsKey (strValue.ToLower ().Trim())) {
							dBSBSemanticTags.Add (strValue.ToLower (), new List<SemanticTag> ());
						}
					}
				}
			}

			//Fill dSemanticParts and dSemanticPartsReversed
			foreach (string strKey in dBSBSemanticTags.Keys.Where(a=>a.Trim() != "").OrderBy(a=>a)) {
				//Console.Write ("strKey:" + strKey + " ");
				foreach (SemanticTag stKey in dBSBSemanticTags[strKey].OrderBy(a=>a.strTranslation)) {
					string strWholeTag = stKey.strTranslation;
					//Console.WriteLine ("strWholeTag:" + strWholeTag + " ");

					foreach (string strWrite in stKey.lTags) {
						if (!strWrite.Contains ('|')) {
							foreach (string strPart in strWrite.Split(',')) {
								if (!dSemanticParts.ContainsKey (strKey.ToLower ())) {
									dSemanticParts.Add (strKey.ToLower (), new List<string> ());
								}

								if (!dSemanticParts [strKey.ToLower ()].Contains (strPart.ToLower ())) {
									dSemanticParts [strKey.ToLower ()].Add (strPart.ToLower ());
								}

								if (!dSemanticPartsReversed.ContainsKey (strPart.ToLower ())) {
									dSemanticPartsReversed.Add (strPart.ToLower (), new List<string> ());
								}

								if (!dSemanticPartsReversed [strPart.ToLower ()].Contains (strKey.ToLower ())) {
									dSemanticPartsReversed [strPart.ToLower ()].Add (strKey.ToLower ());
								}
							}
						}
					}
				}
			}

		}

		public void WriteParsing(ref StreamWriter swParsing){
			foreach (string strKey in dBSBParses.Keys.OrderBy(a=>a)) {
				foreach (string strKey2 in dBSBParses[strKey].OrderBy(a=>a)) {
					swParsing.WriteLine (strKey + " ^ " + strKey2);

				}
			}

			swParsing.Close ();
		}

		public void WriteUnifiedChunksHebrew(ref StreamWriter swUnifiedChunksHebrew){

			foreach (string strWordsOnly in dBSBChunksEH.Keys.OrderBy(a=>a)) {
				foreach (string strTransTemp in dBSBChunksEH[strWordsOnly]) {
					swUnifiedChunksHebrew.WriteLine (strWordsOnly + " ^ " + strTransTemp);
				}
			}

			swUnifiedChunksHebrew.WriteLine ("^");

			foreach (string strTranslit in dBSBChunksHebrew.Keys.OrderBy(a=>a)) {
				foreach (string strWordsTemp in dBSBChunksHebrew[strTranslit]) {
					swUnifiedChunksHebrew.WriteLine (strTranslit + " ^ " + strWordsTemp);
				}
			}

			swUnifiedChunksHebrew.Close ();
		}

		public void WriteUnifiedChunksGreek(ref StreamWriter swUnifiedChunksGreek){
			swUnifiedChunksGreek.WriteLine ("^");

			foreach (string strTranslit in dBSBChunksEG.Keys.OrderBy(a=>a)) {
				foreach (string strWordsTemp in dBSBChunksEG[strTranslit]) {
					swUnifiedChunksGreek.WriteLine (strWordsTemp + " ^ " + strTranslit);
				}
			}

			swUnifiedChunksGreek.WriteLine ("^");

			foreach (string strTranslit in dBSBChunksGreek.Keys.OrderBy(a=>a)) {
				foreach (string strWordsTemp in dBSBChunksGreek[strTranslit]) {
					swUnifiedChunksGreek.WriteLine (strTranslit + " ^ " + strWordsTemp);
				}
			}

			swUnifiedChunksGreek.Close ();
		}

		public void WriteSemanticTags(){
            StreamWriter swSemanticParts = new StreamWriter(strDataPath + @"bsb_tables_6-7-19-SemanticParts.txt");
            StreamWriter swSemanticPartsReversed = new StreamWriter(strDataPath + @"bsb_tables_6-7-19-SemanticPartsReversed.txt");

			foreach (string strPartKey in dSemanticParts.Keys.OrderBy(a=>a)) {
				foreach (string strPartValue in dSemanticParts[strPartKey]) {
					if (strPartValue.ToLower () != "vvv") {
						swSemanticParts.WriteLine (strPartKey.ToLower () + " ^ " + strPartValue.ToLower ());
					}
				}
			}

			foreach (string strPartKey in dSemanticPartsReversed.Keys.OrderBy(a=>a)) {
				foreach (string strPartValue in dSemanticPartsReversed[strPartKey]) {
					if (strPartValue.ToLower () != "vvv") {
						swSemanticPartsReversed.WriteLine (strPartKey.ToLower () + " ^ " + strPartValue.ToLower ());
					}
				}
			}

			swSemanticParts.Close ();
			swSemanticPartsReversed.Close ();
		}
        
		public void WriteBSBRecords(){
            StreamWriter swBSBRecords = new StreamWriter(strDataPath + @"bsb_tables_6-7-19-BSBRecords.txt");

			foreach (int intRecordRank in slBSB.Keys) {
				swBSBRecords.WriteLine (slBSB [intRecordRank].ToRecord ());
			}

			swBSBRecords.Close ();
		}

		public void Group(string strWord, string strGroup){
			if (!dBasicWords.ContainsKey (strWord)) {
				dBasicWords [strWord] = strGroup;
			} else {
				if (dBasicWords [strWord] != strGroup) {
					throw new Exception ("The group for " + strWord + " is " +
					dBasicWords [strWord] + ".  A change of this group to " +
					strGroup + " was rejected.");
				}
			}
		}

		public List<string> GetWordsInGroup(string strGroup){
			List<string> lReturn = new List<string> ();

			foreach (string strWord in dBasicWords.Where(a=>a.Value == strGroup).Select(a=>a.Key)) {
				lReturn.Add (strWord);
			}

			return lReturn;
		}

        //foreach greek word transliteration, collect english translation(s)
        //try to match each part of each similar transliteration (character by character)
        // against each english word
        // A>make longest greek length memory for each letter (or "·"-separated part, as
        //  the data has it), using forward combinations only
        // B>increment count for each english word not already identified as other than the
        //  transliteration at hand
        //for each length of ordered combination of letters in the greek transliteration, 
        // add 1 to the count of each english word in the translation
        public void GreekParts()
        {
            char cTSplit = "·".ToCharArray()[0];

            foreach (int intID in slBSB.Keys.Where(a=>a > 1).OrderBy(a => a))
            {
                string strTransliteration = slBSB[intID].strTransliteration;
                string[] strsTransliteration = strTransliteration.Split(cTSplit);
                int intTransliterationArrayLength = strsTransliteration.Length;
                string strBSBVersion = slBSB[intID].strBSBVersion.ToLower();
                
                for (int intTranslitCounter = 0; intTranslitCounter < intTransliterationArrayLength; intTranslitCounter++)
                {
                    string strPart = strsTransliteration[intTranslitCounter];
                    AddGPEC(strPart, strBSBVersion);

                    //this is nested here because I don't know which syllable(s) are grouped in one -eme (word)
                    for (int intLengthCounter = intTranslitCounter + 1; intLengthCounter < intTransliterationArrayLength; intLengthCounter++)
                    {
                        strPart += strsTransliteration[intLengthCounter];
                        AddGPEC(strPart, strBSBVersion);
                    }
                }
            }

            dOrderedGPEC = GetOrderedGPEC();
            WriteOrderedGPEC(strDataPath + "OrderedGreekPartEnglishCounts.txt");
            //Dictionary<int, Dictionary<string, List<string>>> dOrderedGPEC = GetOrderedGPEC();
        }

        public void WriteOrderedGPEC(string strOrderedGreekPartEnglishCountsFilename)
        {
            StreamWriter swOrderedGPEC = new StreamWriter(strOrderedGreekPartEnglishCountsFilename);

            //Dictionary<string, Dictionary<int, List<string>>> 
            foreach (string strGreekParts in dOrderedGPEC.Keys.OrderBy(a => a))
            {
                foreach (int intCount in dOrderedGPEC[strGreekParts].Keys.OrderBy(a => a))
                {
                    foreach (string strEnglishWord in dOrderedGPEC[strGreekParts][intCount])
                    {
                        swOrderedGPEC.WriteLine(strGreekParts + " ^ " + intCount.ToString() +
                            " ^ " + strEnglishWord);
                    }
                }
            }

            swOrderedGPEC.Close();
        }

        public void AddGPEC(string strLetter, string strBSBVersion)
        {
            if (!dGreekPartEnglishCounts.ContainsKey(strLetter))
            {
                dGreekPartEnglishCounts.Add(strLetter, new Dictionary<string, int>());
            }

            foreach (string strEnglishWord in strBSBVersion.Trim().Split())
            {
                if (!dGreekPartEnglishCounts[strLetter].ContainsKey(strEnglishWord))
                {
                    dGreekPartEnglishCounts[strLetter].Add(strEnglishWord, 0);
                }

                dGreekPartEnglishCounts[strLetter][strEnglishWord]++;
            }
        }

        //public Dictionary<int, Dictionary<string, List<string>>> GetOrderedGPEC()
        public Dictionary<string, Dictionary<int, List<string>>> GetOrderedGPEC()
        {
            //Dictionary<string, Dictionary<string, int>> dReturn = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, List<string>>> dReturn = new Dictionary<int, Dictionary<string, List<string>>>();
            Dictionary<int, Dictionary<string, List<string>>> dReturn2 = new Dictionary<int, Dictionary<string, List<string>>>();
            Dictionary<string, string> dReturn3 = new Dictionary<string, string>(); //D<translitPart, highest count english word>
            Dictionary<string, Dictionary<int, List<string>>> dReturn4 = new Dictionary<string, Dictionary<int, List<string>>>(); //D<translitPart, D<count, L<highest count first english word>>>

            var ord =
                from row in dGreekPartEnglishCounts
                select row.Key;

            foreach (string strK1 in dGreekPartEnglishCounts.Keys)
            {
                foreach (string strK2 in dGreekPartEnglishCounts[strK1].Keys)
                {
                    int intCount = dGreekPartEnglishCounts[strK1][strK2];

                    if (!dReturn.ContainsKey(intCount))
                    {
                        dReturn.Add(intCount, new Dictionary<string, List<string>>());
                    }

                    if (!dReturn[intCount].ContainsKey(strK2))
                    {
                        dReturn[intCount].Add(strK2, new List<string>());
                    }

                    dReturn[intCount][strK2].Add(strK1);
                }
            }

            foreach (int intK1 in dReturn.OrderByDescending(a => a.Key).Select(a => a.Key))
            {
                //dReturn2.Add(intK1, new Dictionary<string, List<string>>());

                foreach (KeyValuePair<string, List<string>> kvpEnglishCounts in dReturn[intK1].OrderBy(a => a.Value.Count).OrderBy(a => a.Key))
                {
                    if (Regex.IsMatch(kvpEnglishCounts.Key, @"[a-zA-Z]"))
                    {
                        //dReturn2[intK1].Add(kvp.Key, kvp.Value);

                        foreach (string strTranslit in kvpEnglishCounts.Value)
                        {
                            //if (!dReturn3.ContainsKey(strTranslit)) //grabs the first ordered one
                            //{
                            //    dReturn3.Add(strTranslit, kvp.Key);
                            //}

                            if (!dReturn4.ContainsKey(strTranslit))
                            {
                                dReturn4.Add(strTranslit, new Dictionary<int, List<string>>());
                            }

                            if (!dReturn4[strTranslit].ContainsKey(intK1))
                            {
                                dReturn4[strTranslit].Add(intK1, new List<string>());
                            }

                            dReturn4[strTranslit][intK1].Add(kvpEnglishCounts.Key);
                        }
                    }
                }
            }

            return dReturn4;
        }

		public void BSBSquared(){
			Dictionary<string, Dictionary<string, int>> dBSBSquared =
				new Dictionary<string, Dictionary<string, int>> ();
			string strPart1 = "";
			string strPart2 = "";
            StreamWriter swBSBSquared = new StreamWriter(strDataPath + @"bsb_tables_6-7-19-BSBSquared.txt");
            //D<
			foreach (int intRank in slBSB.OrderBy(a=>a.Value.strBSort).Select(a=>a.Key)) {
                int intStrong;
                BSBRecord brecBSBSquared = slBSB [intRank];
				brecBSBSquared.CreateDataArray (); //its values have been assigned, so this step can now be taken

                if (brecBSBSquared.strStrong.Trim().Length > 0)
                {
                    intStrong = Convert.ToInt32(brecBSBSquared.strStrong.Trim());
                }
                else
                {
                    intStrong = -1;
                }

                for (int intX1 = 0; intX1 < 15; intX1++)
                {
                    strPart1 = brecBSBSquared.strsRecord[intX1];

                    if (!dBSBSquared.ContainsKey(strPart1))
                    {
                        dBSBSquared.Add(strPart1, new Dictionary<string, int>());
                    }

                    for (int intX2 = 0; intX2 < 15; intX2++)
                    {
                        strPart2 = brecBSBSquared.strsRecord[intX2];

                        if (!dBSBSquared[strPart1].ContainsKey(strPart2))
                        {
                            dBSBSquared[strPart1].Add(strPart2, 0);
                        }
                        else
                        {
                            dBSBSquared[strPart1][strPart2]++;
                        }

                        swBSBSquared.WriteLine(strPart1 + " ^ " + strPart2 + " ^ " + dBSBSquared[strPart1][strPart2].ToString());
                    } //2
                } //1
			}

			swBSBSquared.Close ();
		}

        public StringBuilder MakeEnglish()
        {
            StringBuilder sbReturn = new StringBuilder();

            foreach (int intRecordID in slBSB.Keys.OrderBy(a => a))
            {
                BSBRecord rec = slBSB[intRecordID];

                sbReturn.Append(rec.strBSBVersion);
                sbReturn.Append(' ');
            }

            return sbReturn;
        }
	}

	public class BSBRecord
	{
		public string[] strsRecord = (string[])Array.CreateInstance(typeof(string), 15);
		public string strHebSort, strGreekSort, strBSort, strLanguage, 
		strVerse, strWLC, strSeperator, strTransliteration, strParsing, 
		strStrong, strKJVVerse, strHeading, strBSBVersion, strFootnotes, 
		strBDBThayers = "";

		public void CreateDataArray(){
			strsRecord [0]=strHebSort; //3,7,8
			strsRecord [1]=strGreekSort; //3,7,8
			strsRecord [2]=strBSort; //5,8
			strsRecord [3]=strLanguage;
			strsRecord [4]=strVerse;
			strsRecord [5]=strWLC;
			strsRecord [6]=strSeperator;
			strsRecord [7]=strTransliteration;
			strsRecord [8]=strParsing;
			strsRecord [9]=strStrong;
			strsRecord [10]=strKJVVerse;
			strsRecord [11]=strHeading;
			strsRecord [12]=strBSBVersion;
			strsRecord [13]=strFootnotes;
			strsRecord [14]=strBDBThayers;
		}

		public string ToRecord (){
			return strHebSort + " ^ " + strGreekSort + " ^ " + strBSort + " ^ " +
				strLanguage + " ^ " + strVerse + " ^ " + strWLC + " ^ " +
				strSeperator + " ^ " + strTransliteration + " ^ " + strParsing + " ^ " +
				strStrong + " ^ " + strKJVVerse + " ^ " + strHeading + " ^ " +
				strBSBVersion + " ^ " + strFootnotes + " ^ " + strBDBThayers;
		}

		public string[] FromRecord(string strRecord){
			string[] strsRecord = strRecord.Split ('^');

			strHebSort = strsRecord [0].Trim();
            strGreekSort = strsRecord[1].Trim();
            strBSort = strsRecord[2].Trim();
            strLanguage = strsRecord[3].Trim();
            strVerse = strsRecord[4].Trim();
            strWLC = strsRecord[5].Trim();
            strSeperator = strsRecord[6].Trim();
            strTransliteration = strsRecord[7].Trim();
            strParsing = strsRecord[8].Trim();
            strStrong = strsRecord[9].Trim();
            strKJVVerse = strsRecord[10].Trim();
            strHeading = strsRecord[11].Trim();
            strBSBVersion = strsRecord[12].Trim();
            strFootnotes = strsRecord[13].Trim();
            strBDBThayers = strsRecord[14].Trim();

			return strsRecord;
		}
	}

    public class SemanticPart
    {
        public Dictionary<int, string> dParts = new Dictionary<int, string>();

        public void MakeParts(string strTag)
        {
            if (strTag.Contains(@"-"))
            {
                foreach (string strPart in strTag.Split(@"-".ToCharArray()[0]))
                {
                    dParts.Add(dParts.Count() + 1, strPart.Trim());
                }
            }
            else if (strTag.Contains(@"·"))
            {
                foreach (string strPart in strTag.Split(@"·".ToCharArray()[0]))
                {
                    dParts.Add(dParts.Count() + 1, strPart.Trim());
                }
            }
            else if (strTag.Contains(@","))
            {
                foreach (string strPart in strTag.Split(@",".ToCharArray()[0]))
                {
                    dParts.Add(dParts.Count() + 1, strPart.Trim());
                }
            }
            else if (strTag.Contains(@";"))
            {
                foreach (string strPart in strTag.Split(@";".ToCharArray()[0]))
                {
                    dParts.Add(dParts.Count() + 1, strPart.Trim());
                }
            }
            else if (strTag.Contains(@":"))
            {
                foreach (string strPart in strTag.Split(@":".ToCharArray()[0]))
                {
                    dParts.Add(dParts.Count() + 1, strPart.Trim());
                }
            }
            else
            {
                dParts.Add(dParts.Count() + 1, strTag.Trim());
            }
        }

        public bool IsEqual(SemanticPart smCompare)
        {
            if (dParts.Count() != smCompare.dParts.Count())
            {
                return false;
            }

            foreach (int intPart in dParts.Keys)
            {
                if (dParts[intPart] != smCompare.dParts[intPart])
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasPart(string strPart)
        {
            if (dParts.ContainsValue(strPart))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

	public class SemanticTag
	{
		public List<string> lTags = new List<string>();
		public string strTranslation = "";

		public SemanticTag(string strTranslationIn){
			strTranslation = strTranslationIn.Trim();
		}
	}
}

