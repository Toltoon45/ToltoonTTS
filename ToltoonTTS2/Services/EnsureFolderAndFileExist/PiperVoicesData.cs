using System.IO;

namespace ToltoonTTS2.Services.EnsureFolderAndFileExist
{
    public static class PiperVoicesData
    {
        const string PiperVoices = "ar_JO-kareem-low\r\nar_JO-kareem-medium\r\nbg_BG-dimitar-medium\r\nca_ES-upc_ona-medium\r\nca_ES-upc_ona-x_low\r\nca_ES-upc_pau-x_low\r\ncs_CZ-jirka-low\r\ncs_CZ-jirka-medium\r\ncy_GB-bu_tts-medium\r\ncy_GB-gwryw_gogleddol-medium\r\nda_DK-talesyntese-medium\r\nde_DE-eva_k-x_low\r\nde_DE-karlsson-low\r\nde_DE-kerstin-low\r\nde_DE-mls-medium\r\nde_DE-pavoque-low\r\nde_DE-ramona-low\r\nde_DE-thorsten-high\r\nde_DE-thorsten-low\r\nde_DE-thorsten-medium\r\nde_DE-thorsten_emotional-medium\r\nel_GR-rapunzelina-low\r\nel_GR-rapunzelina-medium\r\nen_GB-alan-low\r\nen_GB-alan-medium\r\nen_GB-alba-medium\r\nen_GB-aru-medium\r\nen_GB-cori-high\r\nen_GB-cori-medium\r\nen_GB-jenny_dioco-medium\r\nen_GB-northern_english_male-medium\r\nen_GB-semaine-medium\r\nen_GB-southern_english_female-low\r\nen_GB-vctk-medium\r\nen_US-amy-low\r\nen_US-amy-medium\r\nen_US-arctic-medium\r\nen_US-bryce-medium\r\nen_US-danny-low\r\nen_US-hfc_female-medium\r\nen_US-hfc_male-medium\r\nen_US-joe-medium\r\nen_US-john-medium\r\nen_US-kathleen-low\r\nen_US-kristin-medium\r\nen_US-kusal-medium\r\nen_US-l2arctic-medium\r\nen_US-lessac-high\r\nen_US-lessac-low\r\nen_US-lessac-medium\r\nen_US-libritts-high\r\nen_US-libritts_r-medium\r\nen_US-ljspeech-high\r\nen_US-ljspeech-medium\r\nen_US-norman-medium\r\nen_US-reza_ibrahim-medium\r\nen_US-ryan-high\r\nen_US-ryan-low\r\nen_US-ryan-medium\r\nen_US-sam-medium\r\nes_AR-daniela-high\r\nes_ES-carlfm-x_low\r\nes_ES-davefx-medium\r\nes_ES-mls_10246-low\r\nes_ES-mls_9972-low\r\nes_ES-sharvard-medium\r\nes_MX-ald-medium\r\nes_MX-claude-high\r\nfa_IR-amir-medium\r\nfa_IR-ganji-medium\r\nfa_IR-ganji_adabi-medium\r\nfa_IR-gyro-medium\r\nfa_IR-reza_ibrahim-medium\r\nfi_FI-harri-low\r\nfi_FI-harri-medium\r\nfr_FR-gilles-low\r\nfr_FR-mls-medium\r\nfr_FR-mls_1840-low\r\nfr_FR-siwis-low\r\nfr_FR-siwis-medium\r\nfr_FR-tom-medium\r\nfr_FR-upmc-medium\r\nhe_IL-motek-medium\r\nhi_IN-pratham-medium\r\nhi_IN-priyamvada-medium\r\nhi_IN-rohan-medium\r\nhu_HU-anna-medium\r\nhu_HU-berta-medium\r\nhu_HU-imre-medium\r\nid_ID-news_tts-medium\r\nis_IS-bui-medium\r\nis_IS-salka-medium\r\nis_IS-steinn-medium\r\nis_IS-ugla-medium\r\nit_IT-paola-medium\r\nit_IT-riccardo-x_low\r\nka_GE-natia-medium\r\nkk_KZ-iseke-x_low\r\nkk_KZ-issai-high\r\nkk_KZ-raya-x_low\r\nlb_LU-marylux-medium\r\nlv_LV-aivars-medium\r\nml_IN-arjun-medium\r\nml_IN-meera-medium\r\nne_NP-chitwan-medium\r\nne_NP-google-medium\r\nne_NP-google-x_low\r\nnl_BE-nathalie-medium\r\nnl_BE-nathalie-x_low\r\nnl_BE-rdh-medium\r\nnl_BE-rdh-x_low\r\nnl_NL-mls-medium\r\nnl_NL-mls_5809-low\r\nnl_NL-mls_7432-low\r\nnl_NL-pim-medium\r\nnl_NL-ronnie-medium\r\nno_NO-talesyntese-medium\r\npl_PL-darkman-medium\r\npl_PL-gosia-medium\r\npl_PL-mc_speech-medium\r\npl_PL-mls_6892-low\r\npt_BR-cadu-medium\r\npt_BR-edresson-low\r\npt_BR-faber-medium\r\npt_BR-jeff-medium\r\npt_PT-tug o-medium\r\nro_RO-mihai-medium\r\nru_RU-denis-medium\r\nru_RU-dmitri-medium\r\nru_RU-irina-medium\r\nru_RU-ruslan-medium\r\nsk_SK-lili-medium\r\nsl_SI-artur-medium\r\nsr_RS-serbski_institut-medium\r\nsv_SE-lisa-medium\r\nsv_SE-nst-medium\r\nsw_CD-lanfrica-medium\r\nte_IN-maya-medium\r\nte_IN-padmavathi-medium\r\nte_IN-venkatesh-medium\r\ntr_TR-dfki-medium\r\nuk_UA-lada-x_low\r\nuk_UA-ukrainian_tts-medium\r\nvi_VN-25hours_single-low\r\nvi_VN-vais1000-medium\r\nvi_VN-vivos-x_low\r\nzh_CN-huayan-medium\r\nzh_CN-huayan-x_low\r\n";

        public static void EnsureFileExists()
        {
            var cwd = Environment.CurrentDirectory;
            var folderPath = Path.Combine(cwd, "models");
            var filePath = Path.Combine(folderPath, "PiperVoices.txt");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, PiperVoices);
                return;
            }

            // Файл есть — проверяем содержимое
            var content = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(content))
            {
                File.WriteAllText(filePath, PiperVoices);
            }
        }
    }
}
