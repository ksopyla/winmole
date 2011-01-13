using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using LuceneStore = Lucene.Net.Store;
using LuceneUtil = Lucene.Net.Util;
using Lucene.Net.Analysis.Standard;
using System.IO;
using Lucene.Net.Documents;
using System.Diagnostics;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis;

namespace TechTests
{

    /// <summary>
    /// Only for testing some features and technologies like lucene.net, indexing, shortcut resolving etc
    /// </summary>
    class Program
    {

        public static List<DocProperties> StartPrograms = new List<DocProperties>(32);
        private static DirectoryInfo INDEX_DIR = new DirectoryInfo(@"d:\winmoleIndex");
        
        static string systemStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
        static string userStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);

        static void Main(string[] args)
        {
            LuceneStore.Directory indexDir = LuceneStore.FSDirectory.Open(INDEX_DIR);
            Analyzer analyzer = new StandardAnalyzer(LuceneUtil.Version.LUCENE_29);

            var lastAccess = INDEX_DIR.LastAccessTime;

            if (lastAccess.AddMinutes(10) < DateTime.Now)
            {
                IndexPrograms(indexDir, analyzer);
            }

            IndexReader reader = IndexReader.Open(LuceneStore.FSDirectory.Open(INDEX_DIR), true); // only searching, so read-only=true

            
            Searcher searcher = new IndexSearcher(reader);
            string[] queryFields = new[] { "analized_path", "name", "orig_path" };
            Stopwatch stwQuery = new Stopwatch();

            var endkey = new ConsoleKeyInfo('q', ConsoleKey.Q,false,false,false);

            Console.WriteLine("Press q-key for end, another key for searching");

            while (Console.ReadKey() != endkey)
            {

                Console.Clear();


                Console.WriteLine("Searching, type a query:");
                string userquery = Console.ReadLine();

                string[] queries = new[] { userquery, userquery, userquery };
               


                //QueryParser parser = new QueryParser(LuceneUtil.Version.LUCENE_29, "analized_path", analyzer);
                //Query query = parser.Parse(userquery);

                stwQuery.Start();
                Query query = MultiFieldQueryParser.Parse(LuceneUtil.Version.LUCENE_29, queries, queryFields, analyzer);

                var tophits = searcher.Search(query, 15);
                stwQuery.Stop();
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("total number of hits {0} in {1} ms", tophits.totalHits,stwQuery.ElapsedMilliseconds);
                Console.ForegroundColor = ConsoleColor.White;


                for (int i = 0; i < tophits.scoreDocs.Length; i++)
                {
                    int docId = tophits.scoreDocs[i].doc;

                    Document doc = searcher.Doc(docId);

                    Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine("\n{0} {1}", i + 1, doc.GetField("name"));
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("------------------------------------------");
                    Console.WriteLine("Path= {0} ", doc.GetField("path"));
                    Console.WriteLine("Analized Path= {0} ", doc.GetField("analized_path"));
                    Console.WriteLine("Orig Path= {0} ", doc.GetField("orig_path"));


                }

                Console.WriteLine("\n Press q-key for end, another key for searching");

            }

            searcher.Close();
            reader.Close();

            Console.ReadKey();
        }

        private static void IndexPrograms(LuceneStore.Directory indexDir, Analyzer analyzer)
        {
            Console.WriteLine("find programs");
 Stopwatch stw = new Stopwatch();
            stw.Start();

            FindPorgrams(userStartMenuPath);
            FindPorgrams(systemStartMenuPath);
            Console.WriteLine("find takes {0}", stw.Elapsed);
            Console.Write("Indexing to directory '" + INDEX_DIR + "'...");

            stw.Restart();

            IndexWriter writer = new IndexWriter(indexDir, analyzer, true, IndexWriter.MaxFieldLength.LIMITED);

            AddToIndex(writer);
            Console.WriteLine(" takes {0} ", stw.Elapsed);
            stw.Restart();
            Console.Write("Optimizing...");
            writer.Optimize();
            writer.Close();
            Console.WriteLine(" takes {0}", stw.Elapsed);
        }

        /// <summary>
        /// Adds to index.
        /// </summary>
        /// <param name="writer">The writer.</param>
        private static void AddToIndex(IndexWriter writer)
        {
            foreach (var indexedDoc in StartPrograms)
            {
                Document doc = new Document();

                doc.Add(new Field("name", indexedDoc.Name.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field("analized_path", indexedDoc.TargetPath.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field("path", indexedDoc.TargetPath, Field.Store.YES, Field.Index.NO));
                doc.Add(new Field("orig_path", indexedDoc.OriginalPath, Field.Store.YES, Field.Index.ANALYZED));

                writer.AddDocument(doc);
            }
        }

        private static void FindPorgrams(string basePath)
        {
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();

            foreach (string file in System.IO.Directory.GetFiles(basePath))
            {
                System.IO.FileInfo fileinfo = new System.IO.FileInfo(file);

                if (fileinfo.Extension.ToLower() == ".lnk")
                {
                    IWshRuntimeLibrary.WshShortcut link = shell.CreateShortcut(file) as IWshRuntimeLibrary.WshShortcut;
                    DocProperties sr = new DocProperties(fileinfo.Name.Substring(0, fileinfo.Name.Length - 4), link.TargetPath, file);
                    StartPrograms.Add(sr);
                }
            }

            // recurse through the subfolders
            foreach (string dir in System.IO.Directory.GetDirectories(basePath))
            {
                FindPorgrams(dir);
            }
        }
    }
}
