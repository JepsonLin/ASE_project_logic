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
using UI;


public enum PARTICIPANT
{
    P_M,
    M_U,
    M_S,
    M_D,
    U_M
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
public enum METHOD_U_M
{
    method_add_to_vocabularybook,
    method_delete_from_vocabularybook,
    method_others
}
public struct IN_MESSAGE
{
    public String wordbook_str;
    public String word_str;
}
public struct OUT_MESSAGE
{
    public String info;
}
public struct MESSAGE
{
    public PARTICIPANT parti;
    public METHOD_M_S method_m_s;
    public METHOD_M_D method_m_d;
    public METHOD_P_M method_p_m;
    public METHOD_M_U method_m_u;
    public METHOD_U_M method_u_m;

    public IN_MESSAGE in_message;
    public OUT_MESSAGE out_message;
}
namespace MASTER_CONTROLL
{
   
    public interface  COMUNICATION_Interface
    {
        MESSAGE message { get; set; }
        void send_messageAsync();
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
        public async void send_messageAsync()
        {
            /*in case that master control is called, first transferring intension to relative call action*/
            switch (message_.parti)
            {
                case PARTICIPANT.U_M:
                    switch (message_.method_u_m)
                    {
                        case METHOD_U_M.method_add_to_vocabularybook:
                            message_.parti = PARTICIPANT.M_S;
                            message_.method_m_s = METHOD_M_S.method_add_word;
                            break;
                        case METHOD_U_M.method_delete_from_vocabularybook:
                            message_.parti = PARTICIPANT.M_S;
                            message_.method_m_s = METHOD_M_S.method_delete_word;
                            break;
                        default:
                            break;
                    }
                    break;

                case PARTICIPANT.P_M:
                    switch (message_.method_p_m)
                    {
                        case METHOD_P_M.method_click_event:
                            //check whether in data base
                            WordListDB wordlistdb_for_PDF = new WordListDB();
                            string result_definition = await wordlistdb_for_PDF.GetWordDefinitionInWordBook(message_.in_message.wordbook_str, message_.in_message.word_str);
                            
                            //if word is in the data base
                            if (result_definition.Length > 2)
                            {
                                var result_definition_temp = new StringBuilder(result_definition);
                                result_definition_temp[1] = '1';
                                result_definition =  result_definition_temp.ToString();
                                //shift to M_U
                                message_.parti = PARTICIPANT.M_U;
                                message_.method_m_u = METHOD_M_U.method_show_definition;
                                message_.in_message.word_str = result_definition;
                                message_.in_message.wordbook_str = "default";
                                
                            }
                            else
                            {
                                //bing look up
                                //then shift to M_U
                                message_.parti = PARTICIPANT.M_U;
                                message_.method_m_u = METHOD_M_U.method_show_definition;
                                message_.in_message.word_str = result_definition;
                                message_.in_message.wordbook_str = "default";
                            }
                           

                            break; //result_show;
                        default:
                            break; //"method without definition";
                    }
                    break;

                default:
                    break;
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            switch (this.message_.parti)
            {
                // LIN JIANPING implementation, communication between master and bing.
                case PARTICIPANT.M_D:
                    switch (this.message_.method_m_d)
                    {
                        case METHOD_M_D.method_lookup:
                            message_.out_message.info = await LexiconQuery.GetLexicon(message_.in_message.word_str);
                            return; //"true";
                            
                        case METHOD_M_D.method_others:
                            return; //"true";
                        default: return; //"method without definition";
                            
                    }
                    
                //communication between Master and data base
                case PARTICIPANT.M_S:
                    //instance object
                    WordListDB wordlistdb = new WordListDB();
                    switch (this.message_.method_m_s)
                    {
                        
                        case METHOD_M_S.method_add_word:
                            message_.out_message.info = await wordlistdb.AddWordToWordBook(message_.in_message.wordbook_str, message_.in_message.word_str);
                            //////////////////////////////
                            //other implementations
                            //////////////////////////////
                            return; //result_add == false ? "false" : "true";
                        case METHOD_M_S.method_lookup_word:
                            message_.out_message.info = await wordlistdb.GetWordDefinitionInWordBook(message_.in_message.wordbook_str, message_.in_message.word_str);
                            return; //message_.out_message.info == null ? "false" : "true";
                        case METHOD_M_S.method_delete_word:
                            message_.out_message.info = await wordlistdb.DeleteWordFromWordBook(message_.in_message.wordbook_str, message_.in_message.word_str);
                            return;
                        default:
                            return; //"method without definition";
                    }
                    
                case PARTICIPANT.M_U:
                    switch (this.message_.method_m_u)
                    {

                        case METHOD_M_U.method_show_definition:

                            App1.Popup instance = new App1.Popup();
                            string result = await instance.wakeUI(message_.in_message.word_str);
                            //////////////////////////////
                            //other implementations
                            //////////////////////////////
                            return; //result;
                    
                        default:
                            return; //"method without definition";
                    }
                default:
                    return; //"participant without definition";

            }
        }
    }
}

