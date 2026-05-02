using System;
using System.Collections.Generic;
using I2.Loc;

namespace PCBSAccess
{
    /// <summary>
    /// Localization for mod-added labels. Supports English (default) and French.
    /// Game text (tutorial body, emails, part names) comes directly from the game — Loc only covers labels the mod adds.
    /// Usage: Loc.Get("key") or Loc.Get("key", arg0, arg1)
    /// </summary>
    public static class Loc
    {
        private static bool _initialized = false;
        private static Dictionary<string, string> _current = null;

        private static readonly Dictionary<string, string> _english = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _french = new Dictionary<string, string>();

        /// <summary>Initializes localization. Call once from Main.Awake().</summary>
        public static void Initialize()
        {
            if (_initialized) return;
            PopulateStrings();
            RefreshLanguage();
            _initialized = true;
        }

        /// <summary>
        /// Refreshes the active language from the game's current setting.
        /// Call if the player changes language at runtime.
        /// </summary>
        public static void RefreshLanguage()
        {
            try
            {
                string lang = LocalizationManager.CurrentLanguage;
                _current = lang == "French" ? _french : _english;
            }
            catch (Exception ex)
            {
                Main.Log?.LogWarning($"[Loc] LocalizationManager unavailable, using English: {ex.Message}");
                _current = _english;
            }
        }

        /// <summary>Returns the localized string for key. Falls back to English, then the key itself.</summary>
        public static string Get(string key)
        {
            if (!_initialized) Initialize();

            if (_current != null && _current.TryGetValue(key, out string val)) return val;
            if (_english.TryGetValue(key, out string eng)) return eng;
            return key;
        }

        /// <summary>Returns the localized string with format arguments substituted ({0}, {1}, ...).</summary>
        public static string Get(string key, params object[] args)
        {
            string template = Get(key);
            try { return string.Format(template, args); }
            catch { return template; }
        }

        private static void Add(string key, string english, string french)
        {
            _english[key] = english;
            _french[key] = french;
        }

        private static void PopulateStrings()
        {
            // Startup
            Add("mod_loaded",   "Accessibility mod loaded. Press S for status.",
                                "Mod d'accessibilité chargé. Appuyez sur S pour le statut.");
            Add("debug_on",     "Debug mode on.",       "Mode débogage activé.");
            Add("debug_off",    "Debug mode off.",      "Mode débogage désactivé.");

            // Scene entry
            Add("scene_workshop_loaded",
                "Workshop loaded. Hover parts to hear tooltips. Press W to repeat last tooltip. Open inventory to pick up parts, then press Enter to select.",
                "Atelier chargé. Survolez les pièces pour entendre les infobulles. W pour répéter. Ouvrez l'inventaire pour prendre des pièces, puis Entrée pour sélectionner.");
            Add("scene_tutorial_loaded",
                "How to Build a PC tutorial loaded. Press T to repeat current task.",
                "Tutoriel Construire un PC chargé. Appuyez sur T pour répéter la tâche en cours.");

            // Career status (S)
            Add("cash",         "Cash",         "Argent");
            Add("kudos",        "Kudos",        "Kudos");
            Add("rating",       "Rating",       "Évaluation");
            Add("stars",        "stars",        "étoiles");

            // Build mode
            Add("installed",    "Installed",    "Installé");
            Add("required",     "Required",     "Requis");
            Add("empty",        "empty",        "vide");
            Add("none",         "none",         "aucun");

            // How to Build a PC tutorial
            Add("hbpc_reread",         "Repeat: {0}",             "Répéter : {0}");
            Add("hbpc_no_task",        "No active task.",          "Aucune tâche active.");
            Add("tutorial_press_enter","Press Enter to continue.", "Appuyez sur Entrée pour continuer.");

            // Main menu
            Add("mainmenu_opened",     "Main menu. Press Up and Down arrows to navigate, Enter to select.",
                                       "Menu principal. Flèches haut bas pour naviguer, Entrée pour sélectionner.");
            Add("mainmenu_help",       "Main menu. Up Down: navigate. Enter: select.",
                                       "Menu principal. Haut Bas : naviguer. Entrée : sélectionner.");
            Add("mainmenu_no_buttons", "No buttons available.",
                                       "Aucun bouton disponible.");
            Add("menu_howtobuild",     "How to build a PC",
                                       "Comment assembler un PC");
            Add("menu_career",         "Classic career.",
                                       "Carrière classique.");
            Add("menu_freebuild",      "Freebuild.",
                                       "Construction libre.");
            Add("menu_esports",        "eSports.",
                                       "eSports.");
            Add("menu_it",             "IT Support.",
                                       "Support informatique.");
            Add("menu_options",        "Options",
                                       "Options");
            Add("menu_exit",           "Exit game",
                                       "Quitter le jeu");
            Add("menu_clicking",       "Activating: {0}.",
                                       "Activation : {0}.");

            // F1 context help strings
            Add("help_workshop",       "Workshop. W: re-read tooltip. I: inventory.",
                                       "Atelier. W : relire info-bulle. I : inventaire.");
            Add("help_options",        "Options. Up Down: navigate. Left Right: adjust. Enter: toggle. Escape: back.",
                                       "Options. Haut Bas : naviguer. Gauche Droite : ajuster. Entrée : activer. Échap : retour.");
            Add("help_bios",           "BIOS. Up Down: settings. Left Right: adjust. Tab: next tab. Enter: activate.",
                                       "BIOS. Haut Bas : paramètres. Gauche Droite : ajuster. Tab : onglet suivant. Entrée : activer.");
            Add("help_careerjob",      "Email inbox. Up Down: navigate. Home End: first/last. Enter: accept job. J: re-read.",
                                       "Boîte mail. Haut Bas : naviguer. Début Fin : premier/dernier. Entrée : accepter. J : relire.");
            Add("help_inventory",      "Inventory. Up Down: browse. Home End: first/last. Enter: select. I: re-read.",
                                       "Inventaire. Haut Bas : parcourir. Début Fin : premier/dernier. Entrée : sélectionner. I : relire.");
            Add("help_saveload",       "Save/Load. Up Down: browse slots. Enter: load. Escape: back. F: re-read.",
                                       "Sauvegarde. Haut Bas : parcourir. Entrée : charger. Échap : retour. F : relire.");
            Add("help_ingamemenu",     "Pause menu. Up Down: navigate. Home End: first/last. Enter: select.",
                                       "Menu pause. Haut Bas : naviguer. Début Fin : premier/dernier. Entrée : sélectionner.");
            Add("help_shop",           "Shop. Up Down: browse. Home End: first/last. Enter: view details. G: re-read.",
                                       "Boutique. Haut Bas : parcourir. Début Fin : premier/dernier. Entrée : voir détails. G : relire.");
            Add("help_workshopsel",    "Workshop selection. Up Down: navigate. Home End: first/last. Enter: select.",
                                       "Sélection atelier. Haut Bas : naviguer. Début Fin : premier/dernier. Entrée : sélectionner.");
            Add("help_messagebox",     "Dialog. Enter: confirm. Escape: cancel.",
                                       "Boîte de dialogue. Entrée : confirmer. Échap : annuler.");
            Add("help_daysummary",     "Day summary. Up Down: navigate. Enter: select.",
                                       "Résumé du jour. Haut Bas : naviguer. Entrée : sélectionner.");
            Add("help_os",             "Desktop. Up Down: navigate programs. Enter: launch. Windows key: start menu.",
                                       "Bureau. Haut Bas : naviguer. Entrée : lancer. Touche Windows : menu démarrer.");
            Add("help_addprogram",     "Programs. Up Down: browse. Home End: first/last. Enter: install. Numpad 5: re-read.",
                                       "Programmes. Haut Bas : parcourir. Début Fin : premier/dernier. Entrée : installer. Pavé 5 : relire.");
            Add("help_musicplayer",    "Music player. Up Down: browse. Home End: first/last. Enter: play.",
                                       "Lecteur audio. Haut Bas : parcourir. Début Fin : premier/dernier. Entrée : lire.");
            Add("help_lighting",       "Lighting. Up Down: browse. Home End: first/last. Space: toggle. Enter: apply.",
                                       "Éclairage. Haut Bas : parcourir. Début Fin : premier/dernier. Espace : activer. Entrée : appliquer.");
            Add("help_pcbay",          "PC Bay. Up Down: browse. Home End: first/last. Enter: select.",
                                       "PC Bay. Haut Bas : parcourir. Début Fin : premier/dernier. Entrée : sélectionner.");
            Add("help_email",          "Email. Up Down: navigate. Home End: first/last. E: re-read. Escape: close.",
                                       "Email. Haut Bas : naviguer. Début Fin : premier/dernier. E : relire. Échap : fermer.");
            Add("help_rank",           "Rankings. Up Down: navigate. Home End: first/last.",
                                       "Classement. Haut Bas : naviguer. Début Fin : premier/dernier.");
            Add("help_jobresult",      "Job result. Enter: dismiss.",
                                       "Résultat emploi. Entrée : fermer.");
            Add("help_workshopnav",    "Workshop. Arrow keys: look around. Enter: interact.",
                                       "Atelier. Flèches : regarder. Entrée : interagir.");

            // Career job inbox
            Add("job_labour",       "Labour",                               "Travail");
            Add("job_kudos",        "Kudos",                                "Kudos");
            Add("job_reread",       "Repeat: {0}",                          "Répéter : {0}");
            Add("job_none",         "No job selected.",                     "Aucun emploi sélectionné.");

            // Cash / kudos change announcements
            Add("career_cash_gained", "Received {0}. Balance: {1}.",        "Reçu {0}. Solde : {1}.");
            Add("career_cash_spent",  "Spent {0}. Balance: {1}.",           "Dépensé {0}. Solde : {1}.");
            Add("career_kudos",       "+{0} kudos. Total: {1}.",            "+{0} kudos. Total : {1}.");

            // Options menu — opened / fallback
            Add("opts_opened",      "Options menu. Press Tab to navigate.",
                                    "Menu options. Appuyez sur Tab pour naviguer.");
            Add("opts_no_controls", "No controls available.", "Aucune option disponible.");

            // Options menu — toggle states
            Add("opt_on",  "On",  "activé");
            Add("opt_off", "Off", "désactivé");

            // Options menu — control labels
            Add("opt_look_sens",   "Look sensitivity",           "Sensibilité vue");
            Add("opt_cursor_sens", "Cursor sensitivity",         "Sensibilité curseur");
            Add("opt_zoom_sens",   "Zoom sensitivity",           "Sensibilité zoom");
            Add("opt_music_vol",   "Music volume",               "Volume musique");
            Add("opt_sound_vol",   "Sound volume",               "Volume son");
            Add("opt_fullscreen",  "Fullscreen",                 "Plein écran");
            Add("opt_vsync",       "V-sync",                     "Synchro verticale");
            Add("opt_invert_y",    "Invert Y",                   "Inverser Y");
            Add("opt_tooltips",    "Tooltips",                   "Infobulles");
            Add("opt_push_scroll", "Push scroll",                "Défilement par pression");
            Add("opt_chroma",      "Razer Chroma",               "Razer Chroma");
            Add("opt_aura",        "ASUS Aura",                  "ASUS Aura");
            Add("opt_hide_tablet", "Hide tablet when minimised", "Masquer tablette réduite");
            Add("opt_language",    "Language",                   "Langue");
            Add("opt_quality",     "Quality",                    "Qualité");
            Add("opt_resolution",  "Resolution",                 "Résolution");
            Add("opt_apply",       "Apply",                      "Appliquer");
            Add("opt_back",        "Back",                       "Retour");

            // Inventory reader
            Add("inv_opened_prefix", "Inventory open.",          "Inventaire ouvert.");
            Add("inv_closed",        "Inventory closed.",        "Inventaire fermé.");
            Add("inv_empty",         "No items.",                "Aucun article.");
            Add("inv_items",         "items",                    "articles");
            Add("inv_reread",        "Repeat: {0}",              "Répéter : {0}");
            Add("inv_no_item",       "No item selected.",        "Aucun article sélectionné.");
            Add("inv_new",           "New",                      "Neuf");
            Add("inv_used",          "Used",                     "Usagé");
            Add("inv_broken",        "Broken",                   "Cassé");
            Add("inv_incompatible",  "Incompatible.",            "Incompatible.");
            Add("inv_cant_select",   "Cannot select item here.", "Impossible de sélectionner cet article ici.");

            // Inventory categories
            Add("inv_cat_all",        "All",                     "Tout");
            Add("inv_cat_misc",       "Misc",                    "Divers");
            Add("inv_cat_cpu",        "CPU",                     "Processeur");
            Add("inv_cat_cooling",    "Cooling",                 "Refroidissement");
            Add("inv_cat_motherboard","Motherboard",             "Carte mère");
            Add("inv_cat_memory",     "Memory",                  "Mémoire");
            Add("inv_cat_gpu",        "GPU",                     "Carte graphique");
            Add("inv_cat_storage",    "Storage",                 "Stockage");
            Add("inv_cat_psu",        "PSU",                     "Alimentation");
            Add("inv_cat_cables",     "Cables",                  "Câbles");
            Add("inv_cat_case",       "Case",                    "Boîtier");
            Add("inv_cat_casecooling","Case Cooling",            "Refroidissement boîtier");
            Add("inv_cat_caseparts",  "Case Parts",              "Pièces boîtier");
            Add("inv_cat_radiators",  "Radiators",               "Radiateurs");
            Add("inv_cat_reservoir",  "Reservoir",               "Réservoir");
            Add("inv_cat_cpublock",   "CPU Block",               "Bloc CPU");
            Add("inv_cat_watergpu",   "Water Cooled GPU",        "GPU watercooling");
            Add("inv_cat_pipes",      "Pipes",                   "Tuyaux");
            Add("inv_cat_pipeconn",   "Pipe Connectors",         "Connecteurs tuyaux");
            Add("inv_cat_coolant",    "Coolant",                 "Liquide de refroidissement");

            // Save/Load menu
            Add("save_menu_save",  "Save game",               "Sauvegarder");
            Add("save_menu_load",  "Load game",               "Charger");
            Add("save_slots",      "saves",                   "sauvegardes");
            Add("save_no_slots",   "No saves.",               "Aucune sauvegarde.");
            Add("save_reread",     "Repeat: {0}",             "Répéter : {0}");
            Add("save_no_slot",    "No save selected.",       "Aucune sauvegarde sélectionnée.");

            // Job status panel
            Add("jobstatus_open",          "Job status.",             "Statut du travail.");
            Add("jobstatus_closed",        "Job status closed.",      "Statut fermé.");
            Add("jobstatus_of",            "of",                      "sur");
            Add("jobstatus_complete",      "objectives complete.",     "objectifs accomplis.");
            Add("jobstatus_press_f7",      "Press N to read objectives.", "N pour lire les objectifs.");
            Add("jobstatus_met",           "Complete",                "Accompli");
            Add("jobstatus_not_met",       "Not complete",            "Non accompli");
            Add("jobstatus_optional",      "Optional, not complete",  "Optionnel, non accompli");
            Add("jobstatus_no_objectives", "No objectives.",          "Aucun objectif.");

            // Shared navigation labels
            Add("nav_of",         "of",                              "sur");
            Add("nav_first_item", "First item.",                     "Premier article.");
            Add("nav_last_item",  "Last item.",                      "Dernier article.");
            Add("inv_label",      "Inventory",                       "Inventaire");

            // Workshop build modes (WorkingOnPC.SetMode)
            Add("mode_assembly",          "Assembly mode.",              "Mode assemblage.");
            Add("mode_disassembly",       "Disassembly mode.",           "Mode désassemblage.");
            Add("mode_cabling",           "Cabling mode.",               "Mode câblage.");
            Add("mode_piping",            "Piping mode.",                "Mode tuyauterie.");
            Add("mode_combo_assembly",    "Auto assembly mode.",         "Mode assemblage automatique.");
            Add("mode_combo_disassembly", "Auto disassembly mode.",      "Mode désassemblage automatique.");

            // Workshop tooltips (WorkshopBuildHandler)
            Add("tooltip_reread", "Repeat: {0}",                     "Répéter : {0}");
            Add("tooltip_none",   "No tooltip.",                     "Aucune info-bulle.");

            // Shop (ShopHandler)
            Add("shop_label",         "Shop",                        "Boutique");
            Add("shop_items",         "items",                       "articles");
            Add("shop_empty",         "No items.",                   "Aucun article.");
            Add("shop_not_open",      "Shop not open.",              "Boutique fermée.");
            Add("shop_reread",        "Repeat: {0}",                 "Répéter : {0}");
            Add("shop_no_item",       "No item selected.",           "Aucun article sélectionné.");
            Add("shop_locked",        "Locked",                      "Verrouillé");
            Add("shop_nav_hint",      "Up Down to browse. Home End for first last.",
                                      "Haut Bas pour naviguer. Début Fin pour premier dernier.");
            Add("shop_added_to_cart",     "{0} added to cart. {1}.",          "{0} ajouté au panier. {1}.");
            Add("shop_cart_empty",        "Cart is empty.",                   "Panier vide.");
            Add("shop_checkout_header",   "Checkout. {0} items.",             "Paiement. {0} articles.");
            Add("shop_total_label",       "Total",                            "Total");
            Add("shop_checkout_total",    "Order total: {0}. Your cash: {1}.","Total commande : {0}. Votre solde : {1}.");
            Add("shop_checkout_hint",     "Press Enter on Buy to confirm.",   "Appuyez sur Entrée sur Acheter pour confirmer.");

            // Workshop selection carousel
            Add("workshop_sel_open",  "Workshop selection. {0} options. Up Down to navigate, Enter to select.",
                                      "Sélection d'atelier. {0} options. Haut Bas pour naviguer, Entrée pour sélectionner.");
            Add("workshop_sel_empty", "No workshops available.", "Aucun atelier disponible.");
            Add("workshop_sel_label", "Workshop",                "Atelier");

            // Day summary screen
            Add("daysummary_open",
                "Day summary. {0}. Cash: {1}. Kudos: {2}.",
                "Résumé du jour. {0}. Argent : {1}. Kudos : {2}.");
            Add("daysummary_nav_hint",
                "Up Down to navigate, Enter to select.",
                "Haut Bas pour naviguer, Entrée pour sélectionner.");
            Add("daysummary_label", "Day summary", "Résumé du jour");

            // Job result screen
            Add("jobresult_success",      "Job completed.",          "Travail terminé.");
            Add("jobresult_failed",        "Job failed.",             "Travail échoué.");
            Add("jobresult_payout",        "Labour: {0}. Total: {1}.", "Travail : {0}. Total : {1}.");
            Add("jobresult_stars",         "{0} stars.",              "{0} étoiles.");
            Add("jobresult_dismiss_hint",  "Press Enter to continue.", "Appuyez sur Entrée pour continuer.");

            // Part install / remove (WorkshopBuildHandler)
            Add("part_installed", "Installed",  "Installé");
            Add("part_removed",   "Removed",    "Retiré");

            // Delivery
            Add("delivery_arrived",  "Delivery waiting. Go to the delivery area to collect it.",
                                     "Livraison en attente. Rendez-vous à la zone de livraison.");
            Add("delivery_manifest", "Delivery collected. {0} items.",
                                     "Livraison collectée. {0} articles.");
            Add("delivery_no_items", "Delivery empty.",   "Livraison vide.");

            // Message box dialogs
            Add("msgbox_hint_ok",     "Press Enter to confirm.",
                                      "Appuyez sur Entrée pour confirmer.");
            Add("msgbox_hint_yesno",  "Press Enter for Yes, Escape for No.",
                                      "Entrée pour Oui, Échap pour Non.");

            // In-game pause menu
            Add("ingamemenu_open",       "Pause menu.",                       "Menu pause.");
            Add("ingamemenu_open_empty", "Pause menu. No buttons.",           "Menu pause. Aucun bouton.");
            Add("ingamemenu_nav_hint",   "Up Down to navigate. Enter to activate.", "Haut Bas pour naviguer. Entrée pour activer.");
            Add("ingamemenu_label",      "Pause menu",                        "Menu pause");
            Add("ingamemenu_unknown",    "Button",                            "Bouton");

            // Career events (CareerEventHandler)
            Add("career_level_up",    "Level up! Now level {0}.",      "Niveau supérieur ! Niveau {0}.");
            Add("career_new_review",  "New review received. Rating: {0} stars.",
                                      "Nouvel avis reçu. Note : {0} étoiles.");

            // 3DMark benchmark (ThreedMarkHandler)
            Add("threedmark_started",       "3DMark benchmark started.",          "Benchmark 3DMark démarré.");
            Add("threedmark_complete",      "Benchmark complete.",                 "Benchmark terminé.");
            Add("threedmark_score_overall", "Overall score: {0}.",                "Score global : {0}.");
            Add("threedmark_score_cpu",     "CPU score: {0}.",                    "Score CPU : {0}.");
            Add("threedmark_score_gpu",     "GPU score: {0}.",                    "Score GPU : {0}.");
            Add("threedmark_cpu_name",      "CPU: {0}.",                          "Processeur : {0}.");
            Add("threedmark_gpu_name",      "GPU: {0}.",                          "Carte graphique : {0}.");
            Add("threedmark_result_summary","Score {0}. CPU {1}. GPU {2}.",       "Score {0}. CPU {1}. GPU {2}.");
            Add("threedmark_reread",        "Repeat: {0}",                        "Répéter : {0}");
            Add("threedmark_none",          "No benchmark result yet.",            "Aucun résultat de benchmark.");

            // OCCT stress test (OCCTHandler)
            Add("occt_started",  "OCCT stress test started.",   "Test de stress OCCT démarré.");
            Add("occt_stopped",  "Stress test stopped.",        "Test de stress arrêté.");
            Add("occt_complete", "OCCT test complete.",         "Test OCCT terminé.");
            Add("occt_reread",   "Repeat: {0}",                 "Répéter : {0}");
            Add("occt_none",     "No OCCT result yet.",         "Aucun résultat OCCT.");
            Add("occt_throttle", "CPU throttling detected.",    "Limitation thermique CPU détectée.");

            // PC Stats build summary (PCStatsHandler)
            Add("pcstats_parts_hint",   "Parts:",               "Pièces :");
            Add("pcstats_dismiss_hint", "Press Enter to close.", "Appuyez sur Entrée pour fermer.");
            Add("pcstats_reread",       "Repeat: {0}",          "Répéter : {0}");
            Add("pcstats_none",         "No build summary.",     "Aucun résumé de build.");

            // Objective completion (ObjectiveHandler)
            Add("objective_complete", "Objective complete: {0}.",
                                      "Objectif accompli : {0}.");

            // HWInfo app (HWInfoHandler)
            Add("hwinfo_open",   "Hardware info.",            "Informations matériel.");
            Add("hwinfo_empty",  "No hardware data.",         "Aucune donnée matériel.");
            Add("hwinfo_reread", "Repeat: {0}",               "Répéter : {0}");
            Add("hwinfo_none",   "No hardware info yet.",     "Aucune info matériel.");

            // Tablet email app (EmailAppHandler)
            Add("email_app_open",      "Email. {0} messages. Up Down to navigate.",
                                       "E-mail. {0} messages. Haut Bas pour naviguer.");
            Add("email_no_selection",  "No email selected.", "Aucun e-mail sélectionné.");

            // PCBay tablet app (PCBayHandler)
            Add("pcbay_buy_open",    "PCBay. Buy. {0} offers. Up Down to browse, Enter for details.",
                                     "PCBay. Acheter. {0} offres. Haut Bas pour naviguer, Entrée pour détails.");
            Add("pcbay_sell_open",   "PCBay. Sell. {0} auctions. Up Down to browse, Enter to act.",
                                     "PCBay. Vendre. {0} enchères. Haut Bas pour naviguer, Entrée pour agir.");
            Add("pcbay_no_offers",   "No offers today.",     "Aucune offre aujourd'hui.");
            Add("pcbay_no_auctions", "No active auctions.",  "Aucune enchère active.");
            Add("pcbay_detail_hint", "Press Add to Cart to buy.",
                                     "Appuyez sur Ajouter au panier pour acheter.");

            // PC power events (PCPowerHandler)
            Add("pc_powered_on",  "PC powered on.",  "PC allumé.");
            Add("pc_powered_off", "PC powered off.", "PC éteint.");

            // Will It Run app (WillItRunHandler)
            Add("wir_pass",         "Compatible.",           "Compatible.");
            Add("wir_fail",         "Not compatible.",       "Non compatible.");
            Add("wir_ok",           "Pass",                  "OK");
            Add("wir_no",           "Fail",                  "Échec");
            Add("wir_needs",        "Needs",                 "Requis");
            Add("wir_has",          "Has",                   "Disponible");
            Add("wir_cat_ram",      "RAM",                   "RAM");
            Add("wir_cat_vram",     "VRAM",                  "VRAM");
            Add("wir_cat_storage",  "Storage",               "Stockage");
            Add("wir_cat_cpu",      "CPU",                   "Processeur");
            Add("wir_cat_gpu",      "GPU",                   "Carte graphique");

            // Add Program tablet app (AddProgramHandler)
            Add("addprog_add_mode",          "Install mode",                      "Mode installation");
            Add("addprog_remove_mode",       "Uninstall mode",                    "Mode désinstallation");
            Add("addprog_open",              "{0}. {1} programs. Up Down to navigate, Enter to install or remove.",
                                             "{0}. {1} programmes. Haut Bas pour naviguer, Entrée pour installer ou désinstaller.");
            Add("addprog_no_item",           "No program selected.",              "Aucun programme sélectionné.");
            Add("addprog_progress",          "{0}: {1}.",                         "{0} : {1}.");
            Add("addprog_restarting",        "Restarting.",                       "Redémarrage.");
            Add("addprog_restart_cancelled", "Restart cancelled.",                "Redémarrage annulé.");

            // Review App (ReviewAppHandler)
            Add("review_open",    "{0}. Rating: {1} stars. {2} reviews. Most recent:",
                                  "{0}. Note : {1} étoiles. {2} avis. Les plus récents :");
            Add("review_updated", "Rating updated: {0} stars.",
                                  "Note mise à jour : {0} étoiles.");
            Add("review_entry",   "{0}, {1}, {2} stars: {3}",
                                  "{0}, {1}, {2} étoiles : {3}");

            // Rank App (RankAppHandler)
            Add("rank_open",        "Parts ranking. {0} items. Up Down to navigate.",
                                    "Classement des pièces. {0} entrées. Haut Bas pour naviguer.");
            Add("rank_item",        "{0}. Rank {1}. {2}. Score: {3}.",
                                    "{0}. Rang {1}. {2}. Score : {3}.");
            Add("rank_empty",       "No parts available in ranking.",
                                    "Aucune pièce dans le classement.");
            Add("rank_entry_label", "Ranking entry",    "Entrée du classement");
            Add("rank_score_label", "Score",            "Score");
            Add("rank_no_item",     "No entry selected.", "Aucune entrée sélectionnée.");

            // Market App (MarketAppHandler)
            Add("market_open",       "Market. Today's prices:",
                                     "Marché. Prix du jour :");
            Add("market_open_empty", "Market data unavailable.",
                                     "Données du marché indisponibles.");

            // OS Desktop (OSHandler)
            Add("os_started",           "Desktop. {0} programs. Up Down to navigate, Enter to launch.",
                                        "Bureau. {0} programmes. Haut Bas pour naviguer, Entrée pour lancer.");
            Add("os_started_empty",     "Desktop. No programs installed.",
                                        "Bureau. Aucun programme installé.");
            Add("os_launched",          "Launching: {0}.",          "Lancement : {0}.");
            Add("os_closed",            "{0} closed.",              "{0} fermé.");
            Add("os_shut_down",         "PC shut down.",            "PC éteint.");
            Add("os_start_menu_opened", "Start menu. {0} programs. Up Down to navigate, Enter to launch, Escape to close.",
                                        "Menu Démarrer. {0} programmes. Haut Bas pour naviguer, Entrée pour lancer, Échap pour fermer.");
            Add("os_start_menu_closed", "Start menu closed.",       "Menu Démarrer fermé.");
            Add("os_no_icons",          "No programs on desktop.",  "Aucun programme sur le bureau.");
            Add("os_icon",              "Program icon",             "Icône de programme.");
            Add("os_startmenu",         "Start menu opened.",       "Menu Démarrer ouvert.");
            Add("os_open_windows",      "Open windows: {0}.",       "Fenêtres ouvertes : {0}.");
            Add("os_no_windows",        "No open windows.",         "Aucune fenêtre ouverte.");
            Add("os_new_email",         "New email received.",      "Nouvel e-mail reçu.");
            Add("os_window_focused",    "{0} focused.",             "{0} au premier plan.");

            // BIOS (BiosHandler)
            Add("bios_open",     "BIOS opened. Tabs: {0}.",
                                 "BIOS ouvert. Onglets : {0}.");
            Add("bios_tab",      "Tab: {0}. {1} settings.",
                                 "Onglet : {0}. {1} paramètres.");
            Add("bios_nav_hint", "Up Down navigate settings. Left Right adjust value. Enter activate. Tab change tab.",
                                 "Haut Bas pour naviguer. Gauche Droite pour ajuster. Entrée pour activer. Tab pour changer d'onglet.");

            // Music Player App (MusicPlayerAppHandler)
            Add("music_playing", "Now playing: track {0}, {1}.",
                                 "Lecture : piste {0}, {1}.");
            Add("music_list",    "Track list. {0} tracks. Up Down to navigate, Enter to play.",
                                 "Liste de pistes. {0} pistes. Haut Bas pour naviguer, Entrée pour lire.");
            Add("music_paused",  "Paused.",  "En pause.");
            Add("music_resumed", "Playing.", "Lecture.");
            Add("music_no_track","No track playing.", "Aucune piste en cours.");
            Add("music_shuffle_on", "Shuffle on.", "Lecture aléatoire activée.");
            Add("music_shuffle_off","Shuffle off.", "Lecture aléatoire désactivée.");
            Add("music_loop_on",    "Loop on.", "Répétition activée.");
            Add("music_loop_off",   "Loop off.", "Répétition désactivée.");
            Add("music_muted",      "Muted.", "Muet.");
            Add("music_unmuted",    "Unmuted.", "Son réactivé.");
            Add("music_volume",     "Volume {0} percent.", "Volume {0} pour cent.");
            Add("music_tab_soundtrack", "Soundtrack.", "Bande sonore.");
            Add("music_tab_userfiles",  "User files.", "Fichiers utilisateur.");
            Add("music_tab_internet",   "Internet.", "Internet.");

            // Lighting App (LightingAppHandler)
            Add("lighting_open",             "Lighting app. {0} LED groups. Up Down to navigate, Space to select.",
                                             "Application éclairage. {0} groupes LED. Haut Bas pour naviguer, Espace pour sélectionner.");
            Add("lighting_selected_state",   "Selected",     "Sélectionné");
            Add("lighting_deselected_state", "Not selected", "Non sélectionné");
            Add("lighting_applied",          "Lighting changes applied.", "Modifications d'éclairage appliquées.");

            // OS handler keys (alternate names used in OSHandler)
            Add("os_booted",          "Desktop. {0} programs. Up Down to navigate, Enter to launch.",
                                      "Bureau. {0} programmes. Haut Bas pour naviguer, Entrée pour lancer.");
            Add("os_desktop",         "Desktop",      "Bureau");
            Add("os_launching",       "Launching: {0}.",  "Lancement : {0}.");
            Add("os_window_closed",   "{0} closed.",      "{0} fermé.");
            Add("os_startmenu_open",  "Start menu. {0} programs. Up Down to navigate, Enter to launch, Escape to close.",
                                      "Menu Démarrer. {0} programmes. Haut Bas pour naviguer, Entrée pour lancer, Échap pour fermer.");
            Add("os_startmenu_closed","Start menu closed.", "Menu Démarrer fermé.");
            Add("os_shutdown",        "PC shut down.",    "PC éteint.");

            // Achievement unlocked (AchievementHandler)
            Add("achievement_unlocked", "Achievement unlocked: {0}.", "Succès débloqué : {0}.");

            // Walking state enter/exit (WalkingStateHandler)
            Add("walking_entered", "Entered workshop. Approach the customer's PC and press E to work on it. Escape for menu.",
                                   "Atelier rejoint. Approchez-vous du PC client et appuyez sur E pour travailler dessus. Échap pour le menu.");
            Add("walking_exited",  "Left workshop.",    "Atelier quitté.");
            Add("walking_interact_hint",    "E: {0}",                                 "E : {0}");
            Add("walking_entered_with_pc", "Entered workshop. Customer PC detected. Press F5 or Numpad2 to teleport to it.",
                                          "Atelier rejoint. PC client détecté. Appuyez sur F5 ou Pavé num 2 pour vous y téléporter.");
            Add("walking_no_customer_pc",  "No customer PC found.",                  "Aucun PC client trouvé.");
            Add("walking_teleported_to_pc","Teleported to customer PC. Press E to interact.", "Téléporté au PC client. Appuyez sur E pour interagir.");
            Add("walking_pc_action",       "{0}.",                                              "{0}.");

            // Working on computer state
            Add("working_on_pc",      "Working on {0}.",  "Travail sur {0}.");
            Add("working_unknown_pc", "PC",               "PC");
            Add("working_done",       "Left PC.",         "PC quitté.");

            // Rename company dialog (RenameCompanyHandler)
            Add("rename_company_open",      "Rename company. Current name: {0}. Enter a new name and press Apply.",
                                            "Renommer l'entreprise. Nom actuel : {0}. Entrez un nouveau nom.");
            Add("rename_company_applied",   "Company name set to {0}.", "Nom de l'entreprise : {0}.");
            Add("rename_company_cancelled", "Cancelled.",               "Annulé.");

            // Notes app (NotesAppHandler)
            Add("notes_open", "Notes. {0} notes.", "Notes. {0} notes.");

            // Select wallpaper app (SelectWallpaperHandler)
            Add("wallpaper_tab_client",   "Client wallpapers.",   "Fonds d'écran client.");
            Add("wallpaper_tab_custom",   "Custom wallpapers.",   "Fonds d'écran personnalisés.");
            Add("wallpaper_tab_workshop", "Workshop wallpapers.", "Fonds d'écran atelier.");
            Add("wallpaper_applied",      "Wallpaper applied.",   "Fond d'écran appliqué.");

            // Freebuild options (FreebuildOptionsHandler)
            Add("freebuild_open", "Freebuild options. {0} tool upgrades.",
                                  "Options Freebuild. {0} améliorations d'outils.");

            // Career inventory updated (CareerEventHandler EVT-3)
            Add("career_inventory_updated", "Inventory updated. {0} items.",
                                            "Inventaire mis à jour. {0} articles.");

            // Workshop Navigator (WorkshopNavigatorHandler)
            Add("ws_assembly_mode",    "Assembly mode. Part: {0}. Tab to cycle slots, Enter to install.",
                                       "Mode assemblage. Pièce : {0}. Tab pour les emplacements, Entrée pour installer.");
            Add("ws_disassembly_mode", "Disassembly mode. Tab to cycle parts, Enter to remove.",
                                       "Mode désassemblage. Tab pour les pièces, Entrée pour retirer.");
            Add("ws_cabling_mode",     "Cabling mode. Tab to cycle connectors, Enter to cable.",
                                       "Mode câblage. Tab pour les connecteurs, Entrée pour câbler.");
            Add("ws_piping_mode",      "Piping mode. Tab to cycle connectors, Enter to connect.",
                                       "Mode tuyauterie. Tab pour les connecteurs, Entrée pour raccorder.");
            Add("ws_thermal_mode",     "Thermal paste mode. Tab to find CPU, Enter to apply.",
                                       "Mode pâte thermique. Tab pour le CPU, Entrée pour appliquer.");
            Add("ws_clean_mode",       "Cleaning mode.",           "Mode nettoyage.");
            Add("ws_no_item",          "No part selected. Open inventory first.",
                                       "Aucune pièce sélectionnée. Ouvrez l'inventaire d'abord.");
            Add("ws_no_item_short",    "none",                     "aucun");
            Add("ws_no_targets",       "No available targets.",    "Aucune cible disponible.");
            Add("ws_target",           "{0} of {1}: {2}",          "{0} sur {1} : {2}");
            Add("ws_installing",       "Installing {0}.",           "Installation de {0}.");
            Add("ws_installing_active","Installing. Please wait.",  "Installation en cours.");
            Add("ws_removing",         "Removing {0}.",             "Retrait de {0}.");
            Add("ws_cant_remove",      "Cannot remove this part.",  "Impossible de retirer cette pièce.");
            Add("ws_open",             " (open)",                   " (ouvert)");
            Add("ws_closed",           " (closed)",                 " (fermé)");
            Add("ws_opening",          "opening",                   "ouverture");
            Add("ws_closing",          "closing",                   "fermeture");
            Add("ws_pin_toggled",      "{0}: {1}.",                 "{0} : {1}.");
            Add("ws_cabling_start",    "Cabling from {0}. Tab to cycle targets, Enter to connect.",
                                       "Câblage depuis {0}. Tab pour les cibles, Entrée pour connecter.");
            Add("ws_piping_start",     "Piping from {0}. Tab to cycle targets, Enter to connect.",
                                       "Tuyauterie depuis {0}. Tab pour les cibles, Entrée pour raccorder.");
            Add("ws_cable_targets",    "{0} cable targets. Tab to cycle, Enter to connect.",
                                       "{0} cibles de câble. Tab pour changer, Entrée pour connecter.");
            Add("ws_pipe_targets",     "{0} pipe targets. Tab to cycle, Enter to connect.",
                                       "{0} cibles de tuyau. Tab pour changer, Entrée pour raccorder.");
            Add("ws_connected",        "Connected.",                "Connecté.");
            Add("ws_cant_connect",     "Cannot connect here.",      "Impossible de connecter ici.");
            Add("ws_orient_hint",      "Part needs orientation. Left/Right to rotate, Enter to accept, R to flip 180.",
                                       "Orientation requise. Gauche/Droite pour tourner, Entrée pour accepter, R pour retourner.");
            Add("ws_orient_accept",    "Orientation accepted.",     "Orientation acceptée.");
            Add("ws_orient_flip",      "Flipped to {0} degrees.",   "Retourné à {0} degrés.");
            Add("ws_orient_angle",     "{0} degrees.",              "{0} degrés.");
            Add("ws_power_button",     "Power button",              "Bouton d'alimentation");
            Add("ws_hbpc_action",      "{0} target(s). {1}. Tab to cycle, Space to interact.",
                                       "{0} cible(s). {1}. Tab pour naviguer, Espace pour interagir.");
            Add("ws_hbpc_hold_space",  "Hold Space to interact.",
                                       "Maintenez Espace pour interagir.");
            Add("ws_hbpc_waiting",     "Waiting for action to complete.", "En attente de l'action.");
            Add("ws_hbpc_inv_btn",     "Part needed from inventory. Press Enter to open inventory.",
                                       "Pièce requise. Appuyez sur Entrée pour ouvrir l'inventaire.");
            Add("ws_hbpc_going_inv",   "Opening inventory.",        "Ouverture de l'inventaire.");
            Add("ws_hbpc_visual_inv",    "Visual inventory: {0} part(s), {1} compatible. Up/Down to navigate, Enter to inspect.",
                                         "Inventaire visuel : {0} pièce(s), {1} compatible(s). Haut/Bas pour naviguer, Entrée pour inspecter.");
            Add("ws_hbpc_inv_item",      "{0} of {1}: {2}. {3}.",
                                         "{0} sur {1} : {2}. {3}.");
            Add("ws_hbpc_compatible",    "Compatible.",               "Compatible.");
            Add("ws_hbpc_not_compat",    "Not compatible.",            "Non compatible.");
            Add("ws_hbpc_inv_none",      "No part focused. Use Up/Down to navigate.",
                                         "Aucune pièce sélectionnée. Utilisez Haut/Bas pour naviguer.");
            Add("ws_hbpc_inspect_open",  "Inspecting: {0}. {1} Escape to go back.",
                                         "Inspection : {0}. {1} Échap pour revenir.");
            Add("ws_hbpc_inspect_use",   "Enter to install.",          "Entrée pour installer.");
            Add("ws_hbpc_installing",    "Installing.",                "Installation.");
            Add("ws_removing_active",    "Removing. Please wait.",     "Retrait en cours.");
            Add("ws_hbpc_inv_back",      "Back.",                      "Retour.");

            // Emergency shutdown / BSOD (PCEmergencyShutdownHandler)
            Add("bsod_announce",   "Emergency shutdown: {0}.",  "Arrêt d'urgence : {0}.");
            Add("bsod_wattage",    "Power supply overloaded.",   "Alimentation surchargée.");
            Add("bsod_cpu_heat",   "CPU overheating.",           "Processeur en surchauffe.");
            Add("bsod_gpu_heat",   "GPU overheating.",           "Carte graphique en surchauffe.");
            Add("bsod_cpu_volt",   "CPU under-voltage.",         "Tension CPU insuffisante.");
            Add("bsod_gpu_volt",   "GPU under-voltage.",         "Tension GPU insuffisante.");
            Add("bsod_ram_volt",   "RAM under-voltage.",         "Tension RAM insuffisante.");

            // Calendar day events (CalendarHandler)
            Add("calendar_no_events", "No events.", "Aucun événement.");

            // OS window focus (WindowFocusHandler)
            Add("window_focused", "{0} focused.", "{0} au premier plan.");

            // GPU Tuner (GPUTunerHandler)
            Add("gpututner_open",        "GPU Tuner. {0}.",                   "Tuner GPU. {0}.");
            Add("gpututner_hint",        "Up Down: cycle params. Left Right: adjust. Enter: apply. R: reset.",
                                         "Haut Bas : paramètre. Gauche Droite : ajuster. Entrée : appliquer. R : réinitialiser.");
            Add("gpututner_applied",     "Settings applied.",                  "Paramètres appliqués.");
            Add("gpututner_reset",       "Reset to defaults.",                 "Réinitialisé aux valeurs par défaut.");
            Add("gpututner_unknown_gpu", "GPU",                                "Carte graphique");
            Add("gpututner_help",        "GPU Tuner. Up Down: cycle params. Left Right: adjust. Enter: apply. R: reset. F1: re-read.",
                                         "Tuner GPU. Haut Bas : paramètre. Gauche Droite : ajuster. Entrée : appliquer. R : réinitialiser. F1 : relire.");

            // HW Monitor live (HWMonitorHandler)
            Add("hwmonitor_open",    "Hardware monitor. {0} sensors. Numpad 7 to read values.",
                                     "Moniteur matériel. {0} capteurs. Pavé 7 pour lire les valeurs.");
            Add("hwmonitor_reading", "Sensors:",           "Capteurs :");
            Add("hwmonitor_reread",  "Repeat: {0}",        "Répéter : {0}");
            Add("hwmonitor_none",    "No sensor data.",     "Aucune donnée de capteur.");

            // Manifest (ManifestHandler)
            Add("manifest_open",  "Delivery. {0} items.",                                  "Livraison. {0} articles.");
            Add("manifest_hint",  "Up Down to review items. Home End for first last.",
                                  "Haut Bas pour parcourir. Début Fin pour premier dernier.");
            Add("manifest_help",  "Manifest. Up Down: navigate. Home End: first/last.",
                                  "Manifeste. Haut Bas : naviguer. Début Fin : premier/dernier.");

            // Key Binding Menu (KeyBindingMenuHandler)
            Add("keybind_open",          "Controls. {0} bindings. Up Down to navigate.",
                                         "Contrôles. {0} liaisons. Haut Bas pour naviguer.");
            Add("keybind_hint",          "Enter to redefine the focused key.",
                                         "Entrée pour redéfinir la touche sélectionnée.");
            Add("keybind_help",          "Controls menu. Up Down: navigate. Enter: redefine.",
                                         "Menu contrôles. Haut Bas : naviguer. Entrée : redéfinir.");
            Add("keybind_item",          "{0} of {1}: {2}. Key: {3}.",
                                         "{0} sur {1} : {2}. Touche : {3}.");
            Add("keybind_item_unbound",  "{0} of {1}: {2}. Not assigned.",
                                         "{0} sur {1} : {2}. Non assignée.");
            Add("keybind_mapping_done",  "Key assigned.",        "Touche assignée.");
            Add("keybind_not_rebindable","Cannot rebind this action.", "Impossible de redéfinir cette action.");

            // What's New popup (WhatsNewPopUpHandler)
            Add("whatsnew_help",         "What's New popup. Enter or Escape to close.",
                                         "Nouveautés. Entrée ou Échap pour fermer.");
            Add("whatsnew_dismiss_hint", "Press Enter to close.",
                                         "Appuyez sur Entrée pour fermer.");

            // Virus Scan app enhanced (VirusScanAppHandler)
            Add("virusscan_help", "Virus scanner. Enter: start scan or dismiss.",
                                  "Antivirus. Entrée : lancer le scan ou fermer.");

            // Notes app navigation (NotesAppHandler)
            Add("notes_compact_hint", "Left Right to browse notes. F1 to re-read.",
                                      "Gauche Droite pour parcourir les notes. F1 pour relire.");
            Add("notes_help",         "Notes. Left Right: browse. F1: re-read.",
                                      "Notes. Gauche Droite : parcourir. F1 : relire.");
            Add("notes_empty",        "No notes.",    "Aucune note.");

            // Freebuild options enhanced (FreebuildOptionsHandler)
            Add("freebuild_hint", "Up Down: navigate. Space: toggle.",
                                  "Haut Bas : naviguer. Espace : activer.");
            Add("freebuild_help", "Freebuild options. Up Down: navigate. Space: toggle upgrade.",
                                  "Options freebuild. Haut Bas : naviguer. Espace : activer.");

            // IT Support lift (ITSupportLiftHandler)
            Add("lift_going_to", "Going to {0}.", "Direction : {0}.");
            Add("lift_arrived",  "Arrived.",      "Arrivé.");

            // LikedIn team selection (LikedInHandler — Esports DLC)
            Add("likedin_open",   "LikedIn. Your likes: {0}. Total: {1}. {2} teams available.",
                                  "LikedIn. Vos likes : {0}. Total : {1}. {2} équipes disponibles.");
            Add("likedin_team",   "{0} of {1}: {2}. Likes required: {3}.",
                                  "{0} sur {1} : {2}. Likes requis : {3}.");
            Add("likedin_chosen", "Team chosen: {0}.",  "Équipe choisie : {0}.");

            // Tablet Shop (TabletShopHandler — DLC shops)
            Add("tabletshop_open",         "Shop. {0} items.",
                                           "Boutique. {0} articles.");
            Add("tabletshop_hint",         "Up Down to browse. F1 to re-read.",
                                           "Haut Bas pour parcourir. F1 pour relire.");
            Add("tabletshop_help",         "DLC Shop. Up Down: navigate. F1: re-read.",
                                           "Boutique DLC. Haut Bas : naviguer. F1 : relire.");
            Add("tabletshop_stock",        "{0} in stock",    "{0} en stock");
            Add("tabletshop_out_of_stock", "out of stock",    "en rupture de stock");
            Add("tabletshop_unavailable",  "Unavailable.",    "Indisponible.");

            // Mobile Phone Messenger (MessengerHandler — Esports DLC)
            Add("messenger_opened",        "Messages. {0}.",              "Messages. {0}.");
            Add("messenger_list",          "Conversations. {0} threads.", "Conversations. {0} fils.");
            Add("messenger_list_unread",   "Conversations. {0} threads. {1} unread.",
                                           "Conversations. {0} fils. {1} non lu(s).");
            Add("messenger_empty",         "No messages.",                "Aucun message.");
            Add("messenger_unknown",       "Unknown",                     "Inconnu");
            Add("messenger_new_msg",       "New message: {0}",            "Nouveau message : {0}");
            Add("messenger_new_msg_named", "{0}: {1}",                    "{0} : {1}");

            // IT Support level on S key
            Add("status_it_level", "Level: {0}.", "Niveau : {0}.");

            // TikkIT Kanban board (TikkITHandler — IT Support DLC)
            Add("tikkit_board_open",    "TikkIT. {0} jobs: {1}.",
                                        "TikkIT. {0} tickets : {1}.");
            Add("tikkit_no_jobs",       "No jobs.",         "Aucun ticket.");
            Add("tikkit_board_hint",    "Left Right: switch column. Up Down: navigate cards. Enter: open card. F1: re-read.",
                                        "Gauche Droite : colonne. Haut Bas : carte. Entrée : ouvrir. F1 : relire.");
            Add("tikkit_board_help",    "TikkIT board. Left Right: columns. Up Down: cards. Enter: open. F1: re-read.",
                                        "Tableau TikkIT. Gauche Droite : colonnes. Haut Bas : cartes. Entrée : ouvrir.");
            Add("tikkit_col_selected",  "{0}. {1} cards.",  "{0}. {1} cartes.");
            Add("tikkit_col_empty",     "{0}: empty.",      "{0} : vide.");
            Add("tikkit_ready",         "Ready to collect", "Prêt à collecter");
            Add("tikkit_popup_closed",  "Card closed.",     "Carte fermée.");
            Add("tikkit_popup_actions", "Enter: {0}. N: {1}. Escape: close.",
                                        "Entrée : {0}. N : {1}. Échap : fermer.");
            Add("tikkit_popup_help",    "Job card. Enter: positive action. N: negative. Escape: close. F1: re-read.",
                                        "Carte ticket. Entrée : action positive. N : action négative. Échap : fermer.");
            Add("tikkit_new_job",       "New IT job: {0} from {1}.",
                                        "Nouveau ticket IT : {0} de {1}.");
            Add("tikkit_none",          "none",             "aucune");

            // Will It Run program list (WillItRunHandler)
            Add("wir_programs",      "Will It Run. {0} programs. Up Down to navigate, Enter to check.",
                                     "Compatibilité. {0} programmes. Haut Bas pour naviguer, Entrée pour vérifier.");
            Add("wir_programs_hint", "Enter to check compatibility.",
                                     "Entrée pour vérifier la compatibilité.");
            Add("wir_help",          "Will It Run. Up Down: navigate programs. Enter: check.",
                                     "Compatibilité. Haut Bas : naviguer. Entrée : vérifier.");

            // Search Filters Panel (SearchFiltersPanelHandler)
            Add("filter_open",      "Filters. {0} options. {1} active.",
                                    "Filtres. {0} options. {1} actifs.");
            Add("filter_changed",   "{0} active filters.",   "{0} filtres actifs.");
            Add("filter_expanded",  "Expanded",              "Développé");
            Add("filter_collapsed", "Collapsed",             "Réduit");
            Add("filter_range",     "Range filter",          "Filtre plage");
            Add("filter_help",      "Filters. Up Down: navigate. Space or Enter: toggle or expand.",
                                    "Filtres. Haut Bas : naviguer. Espace ou Entrée : activer ou développer.");

            // Swap Peripheral State (SwapPeripheralStateHandler)
            Add("swap_perif_empty",   "Swapping {0}. Slot empty.",
                                      "Remplacement {0}. Emplacement vide.");
            Add("swap_perif_current", "Swapping {0}. Current: {1}.",
                                      "Remplacement {0}. Actuellement : {1}.");
            Add("perif_monitor",      "monitor",      "moniteur");
            Add("perif_keyboard",     "keyboard",     "clavier");
            Add("perif_mouse",        "mouse",        "souris");
            Add("perif_mousepad",     "mouse pad",    "tapis de souris");
            Add("perif_headset",      "headset",      "casque");
            Add("perif_microphone",   "microphone",   "microphone");

            // Career status calendar preview (AnnounceCareerStatus)
            Add("status_today",       "Today: {0}.",           "Aujourd'hui : {0}.");
            Add("status_event_today", "Today: {0}.",           "Aujourd'hui : {0}.");
            Add("status_event_in",    "{0}: {1}.",             "{0} : {1}.");
        }
    }
}
