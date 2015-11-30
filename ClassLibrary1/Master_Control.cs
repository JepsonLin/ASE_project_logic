using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading;
using getBing;
using Wordlist.DB;
using App1;


public enum PARTICIPANT
{
    P_M,
    M_U,
    M_S,
    M_D
}

/// <summary>
///  methods for different communication cases
/// </summary>
public enum METHOD_M_S
{
    method_add_wordbook,
    method_delete_wordbook,
    method_add_word,
    method_delete_word,
    method_lookup_word,
    method_others
}
public enum METHOD_M_D
{
   method_lookup,
   method_others
}
public enum METHOD_M_U
{
    method_show_definition,
    method_others
}
public enum METHOD_P_M
{
   method_request_database_word,
   method_click_event,
   method_others
}


public struct MESSAGE
{
    public PARTICIPANT parti;
    public METHOD_M_S method_m_s;
    public METHOD_M_D method_m_d;
    public METHOD_P_M method_p_m;
    public METHOD_M_U method_m_u;

    public String in_message;
    public String out_message;
    public String wordbook_str;
    public String word_str;
}
namespace MASTER_CONTROLL
{
   
    public interface  COMUNICATION_Interface
    {
        MESSAGE message { get; set; }
       Task<string> send_messageAsync();
	    //bool receive_message();
       
    }

  
    public class SPECIFIC_COMMUNICATION : COMUNICATION_Interface
    {
        private MESSAGE message_;
        public MESSAGE message
        {
            get
            {
                return message_;
            }
            set
            {
                message_ = value;
            }
        }
        public async Task<string> send_messageAsync()
        {
          
            switch (this.message_.parti)
            {
                // LIN JIANPING implementation, communication between master and bing.
                case PARTICIPANT.M_D:
                    switch (this.message_.method_m_d)
                    {
                        case METHOD_M_D.method_lookup:
                            message_.out_message = await LexiconQuery.GetLexicon(message_.in_message);
                            return "true";
                            
                        case METHOD_M_D.method_others:
                            return "true";
                        default: return "method without definition";
                            
                    }
                    
                //communication between Master and data base
                case PARTICIPANT.M_S:
                    //instance object
                    WordListDB wordlistdb = WordListDB.instance_;
                    switch (this.message_.method_m_s)
                    {
                        
                        case METHOD_M_S.method_add_word:
                           bool result_add = await wordlistdb.AddWordToWordBook(message_.wordbook_str, message_.word_str);
                            //////////////////////////////
                            //other implementations
                            //////////////////////////////
                            return result_add == false ? "false" : "true";
                        case METHOD_M_S.method_lookup_word:
                            message_.out_message = await wordlistdb.GetWordDefinitionInWordBook(message_.wordbook_str, message_.word_str);
                            return message_.out_message == null ? "false" : "true";
                        default:
                            return "method without definition";
                    }
                    
                case PARTICIPANT.M_U:
                    switch (this.message_.method_m_u)
                    {

                        case METHOD_M_U.method_show_definition:
                            string result = await App1.MainPage.wakeUI(message_.in_message);
                            //////////////////////////////
                            //other implementations
                            //////////////////////////////
                            return result;
                    
                        default:
                            return "method without definition";
                    }
                    
                default:
                    return "participant without definition";

            }
        }
    }
}

/*
namespace COMMASYNC
{
   public class PROGRAM
    {
       
        public  async void addwordtowordbook_async(string wordbookid, string word)
        {
            WordListDB wordlistdb = new WordListDB();
            bool result = await wordlistdb.AddWordToWordBook(wordbookid, word);
            if (result)
            {

            }
            else
            {

            }                
        }
    }
}*/