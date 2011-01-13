using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


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
    /// class which search items in index
    /// </summary>
    public class SearchService : IDisposable
    {
        /// <summary>
        /// index folder
        /// </summary>
        private DirectoryInfo INDEX_DIR = new DirectoryInfo(Properties.Settings.Default.IndexDir);

        string[] queryFields = new[] { "analized_path", "name", "orig_path" };

        #region Lucene fields

        LuceneStore.Directory luceneIndexDir;
        Analyzer analyzer;

        IndexReader reader; // only searching, so read-only=true

        Searcher searcher;
        #endregion

        public SearchService()
        {
            luceneIndexDir = LuceneStore.FSDirectory.Open(INDEX_DIR);
            analyzer = new StandardAnalyzer(LuceneUtil.Version.LUCENE_29);

            reader = IndexReader.Open(luceneIndexDir, true); // only searching, so read-only=true

            searcher = new IndexSearcher(reader);
        }

        public IList<Prompt> Search(string userquery)
        {

            string[] queries = new[] { userquery, userquery, userquery };

            List<Prompt> searchedPrompt = new List<Prompt>(16);

            //QueryParser parser = new QueryParser(LuceneUtil.Version.LUCENE_29, "analized_path", analyzer);
            //Query query = parser.Parse(userquery);

            Query query = MultiFieldQueryParser.Parse(LuceneUtil.Version.LUCENE_29, queries, queryFields, analyzer);

            var tophits = searcher.Search(query, 15);


            for (int i = 0; i < tophits.scoreDocs.Length; i++)
            {
                int docId = tophits.scoreDocs[i].doc;

                Document doc = searcher.Doc(docId);

                Prompt pr = new Prompt();
                pr.Title = doc.GetField("name").StringValue();
                pr.ExecutePath = doc.GetField("path").StringValue();
                pr.FullPath = doc.GetField("orig_path").StringValue();
                searchedPrompt.Add(pr);

            }

            return searchedPrompt;

        }



        public void Dispose()
        {
            if (luceneIndexDir != null)
                luceneIndexDir.Close();

            if (analyzer != null)
                analyzer.Close();

            if (reader != null)
                reader.Close();

            if (searcher != null)
                searcher.Close();

        }
    }
}
