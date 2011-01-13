﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

using Lucene.Net.Index;
using LuceneStore = Lucene.Net.Store;
using LuceneUtil = Lucene.Net.Util;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis;

namespace winmole
{

    /// <summary>
    /// Class for mainatining index
    /// </summary>
    public class IndexingService : IDisposable
    {
        /// <summary>
        /// index folder
        /// </summary>
        private  DirectoryInfo INDEX_DIR = new DirectoryInfo(Properties.Settings.Default.IndexDir);

        /// <summary>
        /// list contains all path for indexing and extension list
        /// </summary>
        Dictionary<string, List<string>> pathToIndex;

        /// <summary>
        /// list with default indexed extension
        /// </summary>
        List<string> allowedExtensions = new List<string>() { ".lnk" };


        /// <summary>
        /// dic with resolvers for different extensions
        /// </summary>
        Dictionary<string, IExtensionResolver> resolvers;


        /// <summary>
        /// List for finded objects to index
        /// </summary>
        List<Prompt> indexedPrompt;

        /// <summary>
        /// Path to "Start Menu" folder for all users
        /// </summary>
        string systemStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);

        /// <summary>
        /// Path to "Start Menu" folder for user
        /// </summary>
        string userStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);

        private TimeSpan IndexValidTime = TimeSpan.FromMinutes(10);

        #region Lucene fields

        LuceneStore.Directory luceneIndexDir;
        Analyzer analyzer;
        #endregion

        public IndexingService()
        {

            luceneIndexDir = LuceneStore.FSDirectory.Open(INDEX_DIR);
            analyzer = new StandardAnalyzer(LuceneUtil.Version.LUCENE_29);

           
            pathToIndex = new Dictionary<string, List<string>>();
            pathToIndex.Add(systemStartMenuPath, allowedExtensions);
            pathToIndex.Add(userStartMenuPath, allowedExtensions);

            resolvers = new Dictionary<string, IExtensionResolver>();
            resolvers.Add("directory", new DirectoryResolver());
            resolvers.Add(".lnk", new SystemLinkResolver());

        }

        /// <summary>
        /// Build the index
        /// </summary>
        public void BuildIndex()
        {
            DateTime lastAccess = INDEX_DIR.LastAccessTime;
            DateTime lastWrite = INDEX_DIR.LastWriteTime;

            if (lastAccess.Add(IndexValidTime) < DateTime.Now)
            {
                indexedPrompt = new List<Prompt>(200);
                foreach (var item in pathToIndex)
                {

                    string path = item.Key;
                    var ext = item.Value;

                    TraverseDirectory(path, ext);

                }

                WriteEntryToIndex();
                indexedPrompt.Clear();
                indexedPrompt = null;

            }

        }

        private void WriteEntryToIndex()
        {
            IndexWriter writer = new IndexWriter(luceneIndexDir, analyzer, true, IndexWriter.MaxFieldLength.LIMITED);

            foreach (var prompt in indexedPrompt)
            {
                Document doc = new Document();

                doc.Add(new Field("name", prompt.Title.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field("analized_path", prompt.ExecutePath.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field("path", prompt.ExecutePath, Field.Store.YES, Field.Index.NO));
                doc.Add(new Field("orig_path", prompt.FullPath, Field.Store.YES, Field.Index.ANALYZED));

                writer.AddDocument(doc);
            }

            writer.Optimize();
            writer.Close();
        }


        private void TraverseDirectory(string basePath, List<string> extensions)
        {

            //1.build Promt for directory
            IExtensionResolver res = resolvers["directory"];

            indexedPrompt.Add(res.BuildPrompt(basePath));

            //2. For all allowed exitensio for this directory
            foreach (var ext in extensions)
            {
                res = resolvers[ext];
                //3. Find all files in direcotry
                foreach (string file in System.IO.Directory.GetFiles(basePath, ext))
                {
                    indexedPrompt.Add(res.BuildPrompt(basePath));
                }
            }

            // recurse through the subfolders
            foreach (string dir in System.IO.Directory.GetDirectories(basePath))
            {
                TraverseDirectory(dir, extensions);
            }
        }


        public void Dispose()
        {
            if (luceneIndexDir != null)
                luceneIndexDir.Close();

            if (analyzer != null)
                analyzer.Close();

            if (indexedPrompt != null)
            {
                indexedPrompt.Clear();
                indexedPrompt = null;
            }
        }
    }
}
