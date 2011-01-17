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
using winmole.Entities;
using System.Diagnostics;

namespace winmole.Logic
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


        /// <summary>
        /// Lucene directory now use RAMDirectory
        /// </summary>
        LuceneStore.Directory luceneIndexDir;
        
        /// <summary>
        /// Analyzer, using StandardAnalyzer
        /// </summary>
        Analyzer analyzer;

        /// <summary>
        /// Index reader
        /// </summary>
        IndexReader reader; // only searching, so read-only=true

        /// <summary>
        /// Searcher for 
        /// </summary>
        Searcher searcher;

        /// <summary>
        /// Query parser
        /// </summary>
        QueryParser parser  ;
        #endregion

        public SearchService()
        {
            var tmpluceneIndexDir = LuceneStore.FSDirectory.Open(INDEX_DIR);

            luceneIndexDir = new LuceneStore.RAMDirectory(tmpluceneIndexDir);
            tmpluceneIndexDir.Close();

            analyzer = new StandardAnalyzer(LuceneUtil.Version.LUCENE_29);

            reader = IndexReader.Open(luceneIndexDir, true); // only searching, so read-only=true

            searcher = new IndexSearcher(reader);

            // parser = new QueryParser(LuceneUtil.Version.LUCENE_29, "analized_path", analyzer);
             parser = new QueryParser(LuceneUtil.Version.LUCENE_29, "name", analyzer);

             parser.SetDefaultOperator(QueryParser.Operator.AND);
             parser.SetAllowLeadingWildcard(true);
        }

        public IList<PromptItem> Search(string userquery)
        {

            //string[] queries = new[] { userquery, userquery, userquery };

            Stopwatch stw = new Stopwatch();
            stw.Start();

            var querySplit = userquery.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            Query mainQuery;

            if (querySplit.Length < 2)
            {
                mainQuery = BuildTermQuery(querySplit[0]);
            }
            else
            {
                BooleanQuery bmainQuery = new BooleanQuery();
                for (int i = 0; i < querySplit.Length; i++)
                {
                    //if (i == (querySplit.Length - 1))
                    //{
                    //    bmainQuery.Add(new PrefixQuery(new Term("name", querySplit[i])), BooleanClause.Occur.MUST);
                    //}
                    //else
                    //    bmainQuery.Add(new FuzzyQuery(new Term("name", querySplit[i])), BooleanClause.Occur.MUST);

                    bmainQuery.Add(BuildTermQuery(querySplit[i]), BooleanClause.Occur.MUST);


                }

                mainQuery = bmainQuery;
            }



            //if (!userquery.EndsWith(" "))
            //{
            //    userquery = userquery + "*";

            //}
            //userquery = userquery.Replace(" ", "~ ");


            List<PromptItem> searchedPrompt = new List<PromptItem>(16);

            TimeSpan prepareTime = stw.Elapsed;
            
            stw.Restart();

            //Query query = parser.GetPrefixQuery("name", userquery);
            
           // Query query = parser.GetFuzzyQuery("analized_path", userquery, 0.4f);
           // Query query = parser.Parse(userquery);

            //Query query = new WildcardQuery(new Term("name", userquery));
            //Query query = new PrefixQuery(new Term("name", userquery));

            Query query = mainQuery;
            stw.Stop();
            var parseTime = stw.Elapsed;

            //var t = new MultiFieldQueryParser()

            //Query query = MultiFieldQueryParser.Parse(LuceneUtil.Version.LUCENE_29, queries, queryFields, analyzer);

            stw.Restart();

            var tophits = searcher.Search(query, 15);
            
            stw.Stop();
            Debug.WriteLine("user query={0} lucene_query={1} search={2} parse={3}  prepare={4}",userquery,query.ToString(), stw.Elapsed,parseTime,prepareTime);

            for (int i = 0; i < tophits.scoreDocs.Length; i++)
            {
                int docId = tophits.scoreDocs[i].doc;

                Document doc = searcher.Doc(docId);

                PromptItem pr = new PromptItem();
                pr.Name = doc.GetField("name").StringValue();
                //pr.ExecutePath = doc.GetField("path").StringValue();
                pr.FullPath = doc.GetField("path").StringValue();
                searchedPrompt.Add(pr);

            }

            return searchedPrompt;

        }

        private static Query BuildTermQuery(string termQuery)
        {
            Query mainQuery;
            if (termQuery.Length < 3)
                mainQuery = new PrefixQuery(new Term("name", termQuery));
            else
            {
                BooleanQuery boolQuery = new BooleanQuery();

                var fuzzQuery = new FuzzyQuery(new Term("name", termQuery));
                var prefQuery = new PrefixQuery(new Term("name", termQuery));
                boolQuery.Add(prefQuery, BooleanClause.Occur.SHOULD);
                boolQuery.Add(fuzzQuery, BooleanClause.Occur.SHOULD);

                mainQuery = boolQuery;

                //first implemetation
                //mainQuery = fuzzQuery;

            }
            return mainQuery;
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
