// ╔══════════════════════════════════════════════════════════════╗
//  NETRA Avatar Optimizer — Version complete
//  Colle dans : Assets/Editor/VRChatAvatarOptimizer.cs
//  Ouvre avec : Tools > NETRA Avatar Optimizer
// ╚══════════════════════════════════════════════════════════════╝

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR

public class VRChatAvatarOptimizer : EditorWindow
{
    // ═════════════════════════════════════════════════════════════
    //  STYLE CONSTANTS
    // ═════════════════════════════════════════════════════════════
    private static readonly Color COL_BG       = new Color(0.118f, 0.128f, 0.148f);
    private static readonly Color COL_BG2      = new Color(0.138f, 0.148f, 0.168f);
    private static readonly Color COL_BG3      = new Color(0.165f, 0.175f, 0.195f);
    private static readonly Color COL_ACCENT   = new Color(0.090f, 0.560f, 0.950f);
    private static readonly Color COL_ACCENT2  = new Color(0.260f, 0.650f, 0.980f);
    private static readonly Color COL_SUCCESS  = new Color(0.280f, 0.820f, 0.520f);
    private static readonly Color COL_WARN     = new Color(0.960f, 0.710f, 0.120f);
    private static readonly Color COL_ERROR    = new Color(0.920f, 0.260f, 0.280f);
    private static readonly Color COL_PBONE    = new Color(0.620f, 0.340f, 0.960f);
    private static readonly Color COL_TEXT     = new Color(0.930f, 0.930f, 0.945f);
    private static readonly Color COL_TEXT_DIM = new Color(0.640f, 0.660f, 0.720f);
    private static readonly Color COL_SEP      = new Color(0.290f, 0.310f, 0.360f);

    private static readonly Color[] RANK_COLS = {
        new Color(0.20f,0.85f,0.45f), new Color(0.40f,0.85f,0.20f),
        new Color(0.96f,0.80f,0.10f), new Color(1.00f,0.50f,0.00f),
        new Color(0.96f,0.25f,0.25f)
    };
    private static readonly string[] RANK_NAMES = { "Excellent","Good","Medium","Poor","Very Poor" };

    // ═════════════════════════════════════════════════════════════
    //  LOCALISATION  (0=FR 1=EN 2=JP 3=KO 4=ES)
    // ═════════════════════════════════════════════════════════════
    private int _lang = 1;
    private static readonly string[] LANG_CODES = { "FR","EN","JP","KO","ES" };

    private static readonly Dictionary<string, string[]> LOC = new Dictionary<string, string[]>
    {
        // ─ App ─────────────────────────────────────────────────────
        {"app.title",        new[]{"NETRA Avatar Optimizer",  "NETRA Avatar Optimizer", "NETRA アバター最適化",  "NETRA 아바타 최적화",    "NETRA Avatar Optimizer"}},
        {"app.title.short",  new[]{"VRC Optimizer",            "VRC Optimizer",           "VRC最適化",             "VRC 최적화",              "VRC Optimizer"}},
        {"btn.undo",         new[]{"↶ Annuler",                "↶ Undo",                  "↶ 元に戻す",            "↶ 실행취소",              "↶ Deshacer"}},
        // ─ Tabs ────────────────────────────────────────────────────
        {"tab.0",  new[]{"Analyse",    "Analysis",  "分析",            "분석",        "Análisis"}},
        {"tab.1",  new[]{"Physics",    "Physics",   "物理",            "물리",        "Física"}},
        {"tab.2",  new[]{"Bones",      "Bones",     "ボーン",          "뼈대",        "Huesos"}},
        {"tab.3",  new[]{"BlendShapes","BlendShapes","ブレンドシェイプ","블렌드쉐이프","BlendShapes"}},
        {"tab.4",  new[]{"Mesh & UV",  "Mesh & UV", "メッシュ & UV",   "메시 & UV",   "Mesh & UV"}},
        {"tab.5",  new[]{"Textures",   "Textures",  "テクスチャ",      "텍스처",      "Texturas"}},
        {"tab.6",  new[]{"Materiaux",  "Materials", "マテリアル",      "재질",        "Materiales"}},
        {"tab.7",  new[]{"Outils",     "Tools",     "ツール",          "도구",        "Herramientas"}},
        {"tab.8",  new[]{"Log",        "Log",       "ログ",            "로그",        "Log"}},
        // ─ UI commun ───────────────────────────────────────────────
        {"avatar.label",       new[]{"Avatar :",              "Avatar:",              "アバター:",        "아바타:",          "Avatar:"}},
        {"avatar.tooltip",     new[]{"Glisse ici le GameObject racine de ton avatar VRChat",
                                     "Drag the root GameObject of your VRChat avatar here",
                                     "VRChatアバターのルートGameObjectをここにドラッグ",
                                     "VRChat 아바타의 루트 GameObject를 여기에 드래그",
                                     "Arrastra aquí el GameObject raíz de tu avatar VRChat"}},
        {"lbl.folder",         new[]{"Dossier :",             "Folder:",              "フォルダ:",        "폴더:",            "Carpeta:"}},
        {"lbl.search",         new[]{"Rechercher :",          "Search:",              "検索:",            "검색:",            "Buscar:"}},
        {"lbl.current",        new[]{"Actuel",                "Current",              "現在",             "현재",             "Actual"}},
        {"lbl.after",          new[]{"Apres",                 "After",                "最適化後",         "최적화 후",        "Después"}},
        {"lbl.total",          new[]{"Total : ",              "Total: ",              "合計: ",           "합계: ",           "Total: "}},
        {"lbl.results",        new[]{"Resultats : ",          "Results: ",            "結果: ",           "결과: ",           "Resultados: "}},
        {"lbl.mesh",           new[]{"Mesh :",                "Mesh:",                "メッシュ:",        "메시:",            "Mesh:"}},
        {"lbl.tool",           new[]{"Outil :",               "Tool:",                "ツール:",          "도구:",            "Herramienta:"}},
        {"lbl.platform",       new[]{"Plateforme",            "Platform",             "プラットフォーム", "플랫폼",           "Plataforma"}},
        {"lbl.stats",          new[]{"📊 STATISTIQUES",       "📊 STATISTICS",        "📊 統計",          "📊 통계",          "📊 ESTADÍSTICAS"}},
        {"lbl.folder.short",   new[]{"Dossier",               "Folder",               "フォルダ",         "폴더",             "Carpeta"}},
        // ─ Boutons communs ─────────────────────────────────────────
        {"btn.cancel",         new[]{"Annuler",               "Cancel",               "キャンセル",       "취소",             "Cancelar"}},
        {"btn.delete",         new[]{"Supprimer",             "Delete",               "削除",             "삭제",             "Eliminar"}},
        {"btn.apply",          new[]{"Appliquer",             "Apply",                "適用",             "적용",             "Aplicar"}},
        {"btn.check.all",      new[]{"✅ Tout cocher",        "✅ Check All",         "✅ 全選択",        "✅ 전체선택",      "✅ Marcar todo"}},
        {"btn.uncheck.all",    new[]{"❌ Tout decocher",      "❌ Uncheck All",       "❌ 全解除",        "❌ 전체해제",      "❌ Desmarcar todo"}},
        {"btn.clear",          new[]{"🧹 Vider",              "🧹 Clear",             "🧹 クリア",        "🧹 비우기",        "🧹 Vaciar"}},
        {"btn.export",         new[]{"Export rapport TXT",    "Export TXT report",    "TXTレポート出力",  "TXT 보고서 내보내기","Exportar informe TXT"}},
        {"btn.scan",           new[]{"🔍 Scanner",            "🔍 Scan",              "🔍 スキャン",      "🔍 스캔",          "🔍 Escanear"}},
        // ─ Messages/Dialogues communs ──────────────────────────────
        {"msg.no.avatar",      new[]{"Assigne un avatar !",   "Assign an avatar!",    "アバターを設定！", "아바타를 지정하세요!", "¡Asigna un avatar!"}},
        {"msg.no.avatar.top",  new[]{"Assigne un avatar en haut.", "Assign an avatar above.", "上でアバターを設定。", "위에서 아바타를 지정하세요.", "Asigna un avatar arriba."}},
        {"dlg.confirm",        new[]{"Confirmation",          "Confirmation",         "確認",             "확인",             "Confirmación"}},
        {"dlg.cancel",         new[]{"Annuler",               "Cancel",               "キャンセル",       "취소",             "Cancelar"}},
        {"dlg.delete",         new[]{"Supprimer",             "Delete",               "削除",             "삭제",             "Eliminar"}},
        {"dlg.done",           new[]{"Termine",               "Done",                 "完了",             "완료",             "Listo"}},
        {"dlg.ok",             new[]{"OK",                    "OK",                   "OK",               "OK",               "OK"}},
        // ─ Mode copie ──────────────────────────────────────────────
        {"copy.mode",          new[]{" Travailler sur une copie (original intact)",
                                     " Work on a copy (original untouched)",
                                     " コピーで作業（オリジナルを保持）",
                                     " 복사본으로 작업 (원본 보존)",
                                     " Trabajar en copia (original intacto)"}},
        {"copy.mode.tooltip",  new[]{"Chaque optimisation sera appliquée à une copie, jamais à l'original",
                                     "Each optimization will be applied to a copy, never to the original",
                                     "各最適化はコピーに適用され、オリジナルには適用されません",
                                     "각 최적화는 복사본에 적용되며 원본에는 적용되지 않습니다",
                                     "Cada optimización se aplicará a una copia, nunca al original"}},
        // ─ Analyse ─────────────────────────────────────────────────
        {"scan.section",      new[]{"🔬 SCANNER L'AVATAR",         "🔬 SCAN AVATAR",          "🔬 アバタースキャン",  "🔬 아바타 스캔",          "🔬 ESCANEAR AVATAR"}},
        {"scan.btn",          new[]{"🔬 Scanner l'avatar",         "🔬 Scan Avatar",           "🔬 アバタースキャン",  "🔬 아바타 스캔",          "🔬 Escanear avatar"}},
        {"scan.all",          new[]{"🚀 Tout optimiser en 1 clic", "🚀 Optimize All (1-click)","🚀 全て最適化",       "🚀 한번에 최적화",        "🚀 Optimizar todo"}},
        {"scan.all.short",    new[]{"🚀 Tout optimiser",           "🚀 Optimize All",          "🚀 全て最適化",       "🚀 한번에 최적화",        "🚀 Optimizar todo"}},
        // ─ Score ───────────────────────────────────────────────────
        {"score.title",       new[]{"📊 SCORE VRCHAT",        "📊 VRCHAT SCORE",       "📊 VRChatスコア",     "📊 VRCHAT 점수",        "📊 PUNTUACIÓN VRCHAT"}},
        {"score.rankCur",     new[]{"RANG ACTUEL",            "CURRENT RANK",          "現在のランク",        "현재 등급",             "RANGO ACTUAL"}},
        {"score.rankAfter",   new[]{"APRES OPTIMISATION",     "AFTER OPTIMIZATION",    "最適化後",            "최적화 후",             "TRAS OPTIMIZACIÓN"}},
        {"score.levels",      new[]{"▲ +{0} niveau(x)",       "▲ +{0} level(s)",       "▲ +{0}段階",          "▲ +{0}단계",            "▲ +{0} nivel(es)"}},
        // ─ Statistiques ────────────────────────────────────────────
        {"stats.title",       new[]{"📈 STATISTIQUES — clique sur Fix","📈 STATISTICS — click Fix","📈 統計 — Fixをクリック","📈 통계 — Fix 클릭",  "📈 ESTADÍSTICAS — clic en Fix"}},
        {"stat.polygons",     new[]{"Polygones",      "Polygons",      "ポリゴン",     "폴리곤",      "Polígonos"}},
        {"stat.materials",    new[]{"Materiaux",      "Materials",     "マテリアル",   "재질",        "Materiales"}},
        {"stat.bones",        new[]{"Os",             "Bones",         "ボーン",       "뼈대",        "Huesos"}},
        {"stat.physbones",    new[]{"PhysBones",      "PhysBones",     "PhysBones",    "PhysBones",   "PhysBones"}},
        {"stat.blendshapes",  new[]{"BlendShapes",    "BlendShapes",   "ブレンドシェイプ","블렌드쉐이프","BlendShapes"}},
        {"stat.lights",       new[]{"Lights",         "Lights",        "ライト",       "조명",        "Luces"}},
        {"stat.particles",    new[]{"Particles",      "Particles",     "パーティクル", "파티클",      "Partículas"}},
        {"stat.audio",        new[]{"AudioSources",   "AudioSources",  "オーディオ",   "오디오",      "AudioSources"}},
        {"stat.cameras",      new[]{"Cameras",        "Cameras",       "カメラ",       "카메라",      "Cámaras"}},
        {"stat.disabled",     new[]{"Desactives",     "Disabled",      "無効",         "비활성화",    "Desactivados"}},
        {"stat.missing",      new[]{"Scripts manquants","Missing Scripts","スクリプト欠損","누락 스크립트","Scripts faltantes"}},
        {"stat.empty",        new[]{"Objets vides",   "Empty Objects", "空オブジェクト","빈 오브젝트", "Objetos vacíos"}},
        // ─ Actions ─────────────────────────────────────────────────
        {"act.optimize",      new[]{"Optimiser",      "Optimize",      "最適化",       "최적화",      "Optimizar"}},
        {"act.dedup",         new[]{"Dedupliquer",    "Dedup",         "重複削除",     "중복제거",    "Dedup"}},
        {"act.manage",        new[]{"Gerer",          "Manage",        "管理",         "관리",        "Gestionar"}},
        {"act.clean",         new[]{"Nettoyer",       "Clean",         "整理",         "정리",        "Limpiar"}},
        {"act.remove",        new[]{"Supprimer",      "Remove",        "削除",         "제거",        "Eliminar"}},
        // ─ Optimisation dialog ─────────────────────────────────────
        {"dlg.optim.title",   new[]{"Optimisation complete",
                                    "Full Optimization",
                                    "完全最適化",
                                    "전체 최적화",
                                    "Optimización completa"}},
        {"dlg.optim.msg",     new[]{"Appliquer TOUTES les optimisations recommandees ?\nCtrl+Z pour annuler.",
                                    "Apply ALL recommended optimizations?\nCtrl+Z to undo.",
                                    "全ての推奨最適化を適用しますか？\nCtrl+Zで元に戻せます。",
                                    "모든 권장 최적화를 적용하시겠습니까?\nCtrl+Z로 되돌릴 수 있습니다.",
                                    "¿Aplicar TODAS las optimizaciones recomendadas?\nCtrl+Z para deshacer."}},
        {"dlg.apply.all",     new[]{"Appliquer tout",  "Apply All",    "全て適用",     "모두 적용",    "Aplicar todo"}},
        // ─ Quest section ───────────────────────────────────────────
        {"quest.section",     new[]{"🎮 CRÉER VERSION QUEST (copie séparée)",
                                    "🎮 CREATE QUEST VERSION (separate copy)",
                                    "🎮 QUESTバージョン作成（別コピー）",
                                    "🎮 QUEST 버전 생성 (별도 복사본)",
                                    "🎮 CREAR VERSIÓN QUEST (copia separada)"}},
        {"quest.info",        new[]{"Crée une copie de l'avatar optimisée Quest. L'original reste intact.\nLes matériaux, prefab et assets Quest sont sauvegardés dans un dossier séparé.",
                                    "Creates an optimized Quest copy of the avatar. Original stays intact.\nMaterials, prefab and Quest assets are saved in a separate folder.",
                                    "Quest最適化版アバターのコピーを作成します。オリジナルはそのまま。\nマテリアル、プレハブ、Questアセットは別フォルダに保存されます。",
                                    "아바타의 Quest 최적화 복사본을 만듭니다. 원본은 그대로 유지됩니다.\n재질, 프리팹 및 Quest 에셋은 별도 폴더에 저장됩니다.",
                                    "Crea una copia optimizada Quest del avatar. El original permanece intacto.\nMateriales, prefab y assets Quest se guardan en una carpeta separada."}},
        {"quest.opt.shaders", new[]{" Shaders → Quest",     " Shaders → Quest",      " シェーダー→Quest",   " 쉐이더→Quest",      " Shaders → Quest"}},
        {"quest.opt.lights",  new[]{" Lights",              " Lights",               " ライト",             " 조명",              " Luces"}},
        {"quest.opt.particles",new[]{" Particles",          " Particles",            " パーティクル",       " 파티클",            " Partículas"}},
        {"quest.opt.audio",   new[]{" AudioSources",        " AudioSources",         " オーディオ",         " 오디오",            " AudioSources"}},
        {"quest.opt.cameras", new[]{" Cameras",             " Cameras",              " カメラ",             " 카메라",            " Cámaras"}},
        {"quest.opt.missing", new[]{" Scripts manquants",   " Missing Scripts",      " 欠損スクリプト",     " 누락 스크립트",     " Scripts faltantes"}},
        {"quest.shader.label",new[]{"Shader Quest :",       "Quest Shader:",         "Questシェーダー:",    "Quest 쉐이더:",      "Shader Quest:"}},
        {"quest.btn",         new[]{"🎮  Créer version Quest","🎮  Create Quest Version","🎮  Questバージョン作成","🎮  Quest 버전 생성","🎮  Crear versión Quest"}},
        // ─ Plateformes ─────────────────────────────────────────────
        {"platform.quest",    new[]{"🕶 QUEST (Android)",    "🕶 QUEST (Android)",    "🕶 QUEST (Android)",  "🕶 QUEST (Android)",  "🕶 QUEST (Android)"}},
        {"platform.pc",       new[]{"🖥 PC (Standalone)",    "🖥 PC (Standalone)",    "🖥 PC (スタンドアロン)","🖥 PC (Standalone)", "🖥 PC (Standalone)"}},
        // ─ Légende ─────────────────────────────────────────────────
        {"legend.good",       new[]{"Bon (>= 80%)",          "Good (>= 80%)",         "良好 (>= 80%)",       "좋음 (>= 80%)",       "Bueno (>= 80%)"}},
        {"legend.medium",     new[]{"Moyen (50-80%)",        "Medium (50-80%)",       "普通 (50-80%)",       "보통 (50-80%)",       "Medio (50-80%)"}},
        {"legend.poor",       new[]{"A optimiser (< 50%)",   "Optimize (< 50%)",      "要最適化 (< 50%)",    "최적화 필요 (< 50%)","Optimizar (< 50%)"}},
        // ─ Physics ─────────────────────────────────────────────────
        {"physics.section",   new[]{"⚙️ COMPOSANTS PHYSICS",       "⚙️ PHYSICS COMPONENTS",     "⚙️ 物理コンポーネント",    "⚙️ 물리 컴포넌트",        "⚙️ COMPONENTES PHYSICS"}},
        {"physics.info",      new[]{"Coche les composants a garder. Decocher = sera supprime a l'application.",
                                    "Check components to keep. Unchecked = will be deleted on apply.",
                                    "保持するコンポーネントにチェック。外すと適用時に削除されます。",
                                    "유지할 컴포넌트를 체크하세요. 해제하면 적용 시 삭제됩니다.",
                                    "Marca los componentes a conservar. Sin marcar = se eliminará al aplicar."}},
        {"physics.preset.btn",new[]{"🎯 Preset Quest auto",       "🎯 Auto Quest Preset",      "🎯 Questプリセット自動",  "🎯 Quest 프리셋 자동",   "🎯 Preset Quest auto"}},
        {"physics.none.found",new[]{"Aucun PhysBone ou Collider trouve.",
                                    "No PhysBone or Collider found.",
                                    "PhysBoneまたはColliderが見つかりません。",
                                    "PhysBone 또는 Collider를 찾을 수 없습니다.",
                                    "No se encontró ningún PhysBone o Collider."}},
        {"physics.physbones", new[]{"🟣 PhysBones",              "🟣 PhysBones",              "🟣 PhysBones",            "🟣 PhysBones",           "🟣 PhysBones"}},
        {"physics.colliders", new[]{"🟢 PhysBone Colliders",     "🟢 PhysBone Colliders",     "🟢 PhysBone Colliders",   "🟢 PhysBone Colliders",  "🟢 PhysBone Colliders"}},
        {"physics.perf",      new[]{"📈 ESTIMATED PERFORMANCE STATS (QUEST)",
                                    "📈 ESTIMATED PERFORMANCE STATS (QUEST)",
                                    "📈 パフォーマンス推定 (QUEST)",
                                    "📈 성능 추정치 (QUEST)",
                                    "📈 RENDIMIENTO ESTIMADO (QUEST)"}},
        {"physics.delete.confirm",new[]{"Supprimer {0} composant(s) ?\nCtrl+Z pour annuler.",
                                        "Delete {0} component(s)?\nCtrl+Z to undo.",
                                        "{0}個のコンポーネントを削除しますか？\nCtrl+Zで元に戻せます。",
                                        "{0}개의 컴포넌트를 삭제하시겠습니까?\nCtrl+Z로 되돌릴 수 있습니다.",
                                        "¿Eliminar {0} componente(s)?\nCtrl+Z para deshacer."}},
        {"physics.kept",      new[]{"gardes",                  "kept",                      "保持",                    "유지",                   "conservados"}},
        // ─ Bones ───────────────────────────────────────────────────
        {"bones.section",     new[]{"🦴 GESTION DES OS",         "🦴 BONE MANAGEMENT",        "🦴 ボーン管理",           "🦴 뼈대 관리",           "🦴 GESTIÓN DE HUESOS"}},
        {"bones.legend.info", new[]{
            "🟢 Skinne = os avec des vertices (anime le mesh directement).\n🟣 PhysBone = a un composant VRCPhysBone dessus.\n🔵 Structurel = parent requis pour maintenir la hierarchie.\n🟠 Non utilise = non reference par aucun mesh ni physique.",
            "🟢 Skinned = bone with vertices weighted to it (moves the mesh directly).\n🟣 PhysBone = has a VRCPhysBone component directly on it.\n🔵 Structural = parent bone required to keep the hierarchy intact.\n🟠 Unused = not referenced by any mesh or physics component.",
            "🟢 スキン = 頂点ウェイトが付いているボーン（メッシュを直接動かす）。\n🟣 PhysBone = VRCPhysBoneコンポーネントが直接付いている。\n🔵 構造 = 階層を維持するために必要な親ボーン。\n🟠 未使用 = メッシュや物理に参照されていない。",
            "🟢 스킨드 = 버텍스 웨이트가 있는 뼈 (메시를 직접 움직임).\n🟣 PhysBone = VRCPhysBone 컴포넌트가 직접 있는 뼈.\n🔵 구조적 = 계층 구조 유지에 필요한 부모 뼈.\n🟠 미사용 = 메시나 물리에 참조되지 않은 뼈.",
            "🟢 Skinned = hueso con vertices ponderados (mueve el mesh directamente).\n🟣 PhysBone = tiene un componente VRCPhysBone directamente.\n🔵 Estructural = hueso padre necesario para mantener la jerarquia.\n🟠 Sin usar = no referenciado por ningun mesh ni fisica."}},
        {"bones.scan.btn",    new[]{"Scanner",                  "Scan",                      "スキャン",                "스캔",                   "Escanear"}},
        {"bones.merge.btn",   new[]{"Merge inutilises",         "Merge Unused",              "未使用をマージ",          "미사용 병합",            "Fusionar sin usar"}},
        {"bones.dedup.btn",   new[]{"Supprimer doublons",       "Remove Duplicates",         "重複を削除",              "중복 제거",              "Eliminar duplicados"}},
        {"bones.none.found",  new[]{"Aucun os trouve.",         "No bones found.",           "ボーンが見つかりません。","뼈대를 찾을 수 없습니다.", "No se encontraron huesos."}},
        {"bones.skinned",     new[]{"Skinnes : ",               "Skinned: ",                 "スキン: ",                "스킨드: ",               "Con skin: "}},
        {"bones.physbones",   new[]{"PhysBones : ",             "PhysBones: ",               "PhysBones: ",             "PhysBones: ",            "PhysBones: "}},
        {"bones.structural",  new[]{"Structurels : ",           "Structural: ",              "構造: ",                  "구조적: ",               "Estructurales: "}},
        {"bones.unused",      new[]{"Non utilises : ",          "Unused: ",                  "未使用: ",                "미사용: ",               "Sin usar: "}},
        {"bones.filter.all",  new[]{"Tous",                     "All",                       "全て",                    "전체",                   "Todos"}},
        {"bones.filter.skinned",new[]{"Skinnes",                "Skinned",                   "スキン",                  "스킨드",                 "Con skin"}},
        {"bones.filter.physics",new[]{"PhysBones",              "PhysBones",                 "PhysBones",               "PhysBones",              "PhysBones"}},
        {"bones.filter.structural",new[]{"Structurels",         "Structural",                "構造",                    "구조적",                 "Estructurales"}},
        {"bones.filter.unused",new[]{"Non utilises",            "Unused",                    "未使用",                  "미사용",                 "Sin usar"}},
        {"bones.filter.mesh", new[]{"Filtrer par mesh :",       "Filter by mesh:",           "メッシュでフィルタ:",     "메시 필터:",             "Filtrar por mesh:"}},
        {"bones.uncheck.unused",new[]{"Decocher non utilises",  "Uncheck Unused",            "未使用のチェックを外す",  "미사용 해제",            "Desmarcar sin usar"}},
        {"bones.delete.confirm",new[]{"Supprimer {0} os ?\nCtrl+Z pour annuler.",
                                      "Delete {0} bone(s)?\nCtrl+Z to undo.",
                                      "{0}個のボーンを削除しますか？\nCtrl+Zで元に戻せます。",
                                      "{0}개의 뼈대를 삭제하시겠습니까?\nCtrl+Z로 되돌릴 수 있습니다.",
                                      "¿Eliminar {0} hueso(s)?\nCtrl+Z para deshacer."}},
        {"bones.usage.skinned",   new[]{"skinne",               "skinned",                   "スキン",                  "스킨드",                 "skinned"}},
        {"bones.usage.physbone",  new[]{"physbone",             "physbone",                  "physbone",                "physbone",               "physbone"}},
        {"bones.usage.structural",new[]{"structurel",           "structural",                "構造",                    "구조적",                 "estructural"}},
        {"bones.usage.unused",    new[]{"non utilise",          "unused",                    "未使用",                  "미사용",                 "sin usar"}},
        // ─ Mesh ────────────────────────────────────────────────────
        {"mesh.section",         new[]{"🎨 COMPRESSION MESH",       "🎨 MESH COMPRESSION",      "🎨 メッシュ圧縮",         "🎨 메시 압축",           "🎨 COMPRESIÓN DE MESH"}},
        {"mesh.quality",         new[]{"Qualite (%)",               "Quality (%)",              "品質 (%)",                "품질 (%)",               "Calidad (%)"}},
        {"mesh.tris.quest",      new[]{"Triangles max Quest",        "Max Triangles Quest",      "最大三角形数 Quest",      "최대 삼각형 수 Quest",   "Triángulos máx Quest"}},
        {"mesh.tris.pc",         new[]{"Triangles max PC",           "Max Triangles PC",         "最大三角形数 PC",         "최대 삼각형 수 PC",      "Triángulos máx PC"}},
        {"mesh.blend.label",     new[]{"Blend shapes",              "Blend shapes",             "ブレンドシェイプ",        "블렌드쉐이프",           "Blend shapes"}},
        {"mesh.apply.btn",       new[]{"⚡ Appliquer compression",   "⚡ Apply Compression",     "⚡ 圧縮を適用",           "⚡ 압축 적용",           "⚡ Aplicar compresión"}},
        {"mesh.copy.before",     new[]{"Créer une copie du mesh avant compression", "Create mesh copy before compression", "圧縮前にメッシュのコピーを作成", "압축 전에 메시 복사 생성", "Crear copia del mesh antes de la compresión"}},
        {"mesh.combine.section", new[]{"🧩 MESH COMBINING",         "🧩 MESH COMBINING",        "🧩 メッシュ結合",         "🧩 메시 결합",           "🧩 COMBINACIÓN DE MESH"}},
        {"mesh.combine.info",    new[]{"Selectionne les SkinnedMeshRenderers a fusionner en un seul (reduit les draw calls).",
                                       "Select SkinnedMeshRenderers to merge into one (reduces draw calls).",
                                       "結合するSkinnedMeshRendererを選択（ドローコール削減）。",
                                       "하나로 합칠 SkinnedMeshRenderer를 선택하세요 (드로우콜 감소).",
                                       "Selecciona los SkinnedMeshRenderers a fusionar en uno (reduce draw calls)."}},
        {"mesh.add.smr",         new[]{"➕ Ajouter mesh",           "➕ Add mesh",              "➕ メッシュを追加",        "➕ 메시 추가",           "➕ Añadir mesh"}},
        {"mesh.combine.btn",     new[]{"🔗 Combiner les meshes selectionnes","🔗 Combine selected meshes","🔗 選択メッシュを結合", "🔗 선택한 메시 결합",   "🔗 Combinar meshes seleccionados"}},
        {"mesh.hidden.section",  new[]{"🔎 DETECTION POLYGONES CACHES","🔎 HIDDEN POLYGON DETECTION","🔎 非表示ポリゴン検出", "🔎 숨겨진 폴리곤 감지",  "🔎 DETECCIÓN DE POLÍGONOS OCULTOS"}},
        {"mesh.hidden.info",     new[]{"Detecte les triangles a l'interieur du mesh qui ne sont jamais visibles (comme le corps sous les vetements).",
                                       "Detects triangles inside the mesh that are never visible (like body under clothing).",
                                       "衣服の下の体など、決して見えないメッシュ内部の三角形を検出します。",
                                       "옷 아래 신체처럼 절대 보이지 않는 메시 내부 삼각형을 감지합니다.",
                                       "Detecta triángulos dentro del mesh que nunca son visibles (como el cuerpo bajo la ropa)."}},
        {"mesh.hidden.detect.btn",new[]{"🔎 Detecter polygones internes","🔎 Detect hidden polygons","🔎 非表示ポリゴン検出","🔎 숨겨진 폴리곤 감지",   "🔎 Detectar polígonos ocultos"}},
        {"mesh.uv.section",      new[]{"🖌️ SUPPRESSION DE POLYGONES PAR PEINTURE UV",
                                       "🖌️ POLYGON REMOVAL BY UV PAINTING",
                                       "🖌️ UVペイントによるポリゴン削除",
                                       "🖌️ UV 페인팅으로 폴리곤 제거",
                                       "🖌️ ELIMINACIÓN DE POLÍGONOS POR PINTURA UV"}},
        {"mesh.uv.info",         new[]{"Peins sur la map UV pour marquer les polygones a supprimer. 3 modes disponibles.",
                                       "Paint on the UV map to mark polygons for removal. 3 modes available.",
                                       "UVマップ上をペイントして削除するポリゴンをマーク。3モード利用可能。",
                                       "UV 맵을 페인트하여 제거할 폴리곤을 표시합니다. 3가지 모드 사용 가능.",
                                       "Pinta en el mapa UV para marcar polígonos a eliminar. 3 modos disponibles."}},
        {"uv.tool.brush",        new[]{"Pinceau",                   "Brush",                    "ブラシ",                  "브러시",                 "Pincel"}},
        {"uv.tool.rect",         new[]{"Rectangle",                 "Rectangle",                "長方形",                  "직사각형",               "Rectángulo"}},
        {"uv.tool.lasso",        new[]{"Lasso",                     "Lasso",                    "なげなわ",                "올가미",                 "Lazo"}},
        {"uv.brush.size",        new[]{"Taille pinceau",            "Brush size",               "ブラシサイズ",            "브러시 크기",            "Tamaño de pincel"}},
        {"uv.opacity",           new[]{"Opacite",                   "Opacity",                  "不透明度",                "불투명도",               "Opacidad"}},
        {"uv.paint.active",      new[]{"Mode peinture : ACTIF",     "Paint mode: ACTIVE",       "ペイントモード：有効",    "페인트 모드: 활성",      "Modo pintura: ACTIVO"}},
        {"uv.paint.start",       new[]{"Activer peinture",          "Enable painting",          "ペイントを有効化",        "페인팅 활성화",          "Activar pintura"}},
        {"uv.marked.tris",       new[]{"Triangles marques",         "Marked triangles",         "マークされた三角形",      "표시된 삼각형",          "Triángulos marcados"}},
        {"uv.total.tris",        new[]{"Total triangles",           "Total triangles",          "三角形合計",              "삼각형 합계",            "Triángulos totales"}},
        // ─ Textures ────────────────────────────────────────────────
        {"tex.section",          new[]{"🖼️ PARAMETRES GLOBAUX",     "🖼️ GLOBAL SETTINGS",       "🖼️ グローバル設定",       "🖼️ 전역 설정",           "🖼️ PARÁMETROS GLOBALES"}},
        {"tex.scan.btn",         new[]{"🔍 Scanner textures",       "🔍 Scan Textures",         "🔍 テクスチャスキャン",   "🔍 텍스처 스캔",         "🔍 Escanear texturas"}},
        {"tex.scan.unused.btn",  new[]{"🧹 Scanner inutilisees",    "🧹 Scan Unused",           "🧹 未使用をスキャン",     "🧹 미사용 스캔",         "🧹 Escanear no usadas"}},
        {"tex.apply.all.btn",    new[]{"✅ Appliquer a tout",       "✅ Apply to All",          "✅ 全てに適用",           "✅ 전체 적용",           "✅ Aplicar a todo"}},
        {"tex.unused.section",   new[]{"📦 TEXTURES INUTILISEES",   "📦 UNUSED TEXTURES",       "📦 未使用テクスチャ",     "📦 미사용 텍스처",       "📦 TEXTURAS NO USADAS"}},
        {"tex.delete.unused.btn",new[]{"Supprimer les textures inutilisees","Delete unused textures","未使用テクスチャを削除","미사용 텍스처 삭제",   "Eliminar texturas no usadas"}},
        // ─ Matériaux ───────────────────────────────────────────────
        {"mat.section",          new[]{"🎨 GESTION DES MATERIAUX",  "🎨 MATERIAL MANAGEMENT",   "🎨 マテリアル管理",       "🎨 재질 관리",           "🎨 GESTIÓN DE MATERIALES"}},
        {"mat.scan.btn",         new[]{"🔍 Scanner les materiaux",  "🔍 Scan Materials",        "🔍 マテリアルスキャン",   "🔍 재질 스캔",           "🔍 Escanear materiales"}},
        {"mat.unique",           new[]{"Materiaux uniques",         "Unique Materials",         "ユニークなマテリアル",    "고유 재질",              "Materiales únicos"}},
        {"mat.slots",            new[]{"Slots totaux",              "Total Slots",              "スロット合計",            "총 슬롯",                "Slots totales"}},
        {"mat.shaders",          new[]{"Shaders uniques",           "Unique Shaders",           "ユニークなシェーダー",    "고유 쉐이더",            "Shaders únicos"}},
        {"mat.nonquest",         new[]{"Non-Quest",                 "Non-Quest",                "非Quest",                 "비Quest",                "No-Quest"}},
        {"mat.list.section",     new[]{"📋 LISTE DES MATERIAUX",   "📋 MATERIAL LIST",         "📋 マテリアル一覧",       "📋 재질 목록",           "📋 LISTA DE MATERIALES"}},
        {"mat.list.search",      new[]{"Rechercher...",             "Search...",                "検索...",                 "검색...",                "Buscar..."}},
        {"mat.dedup.section",    new[]{"🔀 DEDUPLIQUER",            "🔀 DEDUPLICATE",           "🔀 重複削除",             "🔀 중복제거",            "🔀 DEDUPLICAR"}},
        {"mat.dedup.info",       new[]{"Fusionne les materiaux identiques (meme nom + meme shader) en un seul slot.",
                                       "Merges identical materials (same name + same shader) into a single slot.",
                                       "同一マテリアル（同名＋同シェーダー）を1スロットに統合します。",
                                       "동일한 재질(같은 이름 + 같은 쉐이더)을 하나의 슬롯으로 병합합니다.",
                                       "Fusiona materiales idénticos (mismo nombre + mismo shader) en un único slot."}},
        {"mat.dedup.btn",        new[]{"Dedupliquer maintenant",    "Deduplicate Now",          "今すぐ重複削除",          "지금 중복제거",          "Deduplicar ahora"}},
        {"mat.fix.section",      new[]{"🔧 FIX SHADERS QUEST",      "🔧 FIX QUEST SHADERS",     "🔧 Questシェーダー修正",  "🔧 Quest 쉐이더 수정",   "🔧 FIX SHADERS QUEST"}},
        {"mat.fix.info",         new[]{"Les shaders PC ne fonctionnent pas sur Quest. Remplace-les par des shaders VRChat/Mobile.\nUn nouveau materiau .mat sera cree pour chaque remplacement — l'original est conserve.",
                                       "PC shaders don't work on Quest. Replace them with VRChat/Mobile shaders.\nA new .mat material will be created for each replacement — original is preserved.",
                                       "PCシェーダーはQuestで動作しません。VRChat/Mobileシェーダーに置き換えます。\n各置換に新しい.matが作成されます（オリジナルは保持）。",
                                       "PC 쉐이더는 Quest에서 작동하지 않습니다. VRChat/Mobile 쉐이더로 교체합니다.\n각 교체에 새로운 .mat이 생성됩니다 (원본 보존).",
                                       "Los shaders PC no funcionan en Quest. Reemplázalos por shaders VRChat/Mobile.\nSe creará un nuevo .mat por cada reemplazo — el original se conserva."}},
        {"mat.fix.scan.btn",     new[]{"Scanner shaders incompatibles","Scan incompatible shaders","非互換シェーダーをスキャン","비호환 쉐이더 스캔",   "Escanear shaders incompatibles"}},
        {"mat.fix.shader.target",new[]{"Shader cible",              "Target Shader",            "対象シェーダー",          "대상 쉐이더",            "Shader destino"}},
        {"mat.fix.all.ok",       new[]{"✅  Tous les materiaux sont compatibles Quest.",
                                       "✅  All materials are Quest-compatible.",
                                       "✅  全マテリアルがQuest互換です。",
                                       "✅  모든 재질이 Quest 호환입니다.",
                                       "✅  Todos los materiales son compatibles con Quest."}},
        {"mat.tex.compress",     new[]{"Aussi compresser les textures",  "Also compress textures",  "テクスチャも圧縮",    "텍스처도 압축",          "También comprimir texturas"}},
        // ─ Outils ──────────────────────────────────────────────────
        {"tools.presets.section",new[]{"🛠️ PRESETS RAPIDES",        "🛠️ QUICK PRESETS",         "🛠️ クイックプリセット",   "🛠️ 빠른 프리셋",         "🛠️ PRESETS RÁPIDOS"}},
        {"tools.backup.section", new[]{"💾 BACKUP",                  "💾 BACKUP",                "💾 バックアップ",         "💾 백업",                "💾 BACKUP"}},
        {"tools.backup.toggle",  new[]{"Creer backup avant optimisation","Create backup before optimization","最適化前にバックアップ作成","최적화 전 백업 생성",  "Crear backup antes de optimizar"}},
        {"tools.clean.section",  new[]{"🧹 NETTOYAGE",               "🧹 CLEANUP",               "🧹 クリーンアップ",       "🧹 정리",                "🧹 LIMPIEZA"}},
        {"tools.clean.missing",  new[]{"Scripts manquants",          "Missing scripts",          "欠損スクリプト",          "누락 스크립트",          "Scripts faltantes"}},
        {"tools.clean.empty",    new[]{"Objets vides",               "Empty objects",            "空オブジェクト",          "빈 오브젝트",            "Objetos vacíos"}},
        {"tools.clean.dedup",    new[]{"Dedupliquer materiaux",      "Deduplicate materials",    "マテリアル重複削除",      "재질 중복제거",          "Deduplicar materiales"}},
        {"tools.clean.audio",    new[]{"Supprimer AudioSources",     "Remove AudioSources",      "AudioSourceを削除",       "AudioSource 제거",       "Eliminar AudioSources"}},
        {"tools.clean.cameras",  new[]{"Supprimer Cameras",          "Remove Cameras",           "カメラを削除",            "카메라 제거",            "Eliminar Cámaras"}},
        {"tools.clean.now.btn",  new[]{"Optimiser materiaux maintenant","Optimize materials now","マテリアルを今すぐ最適化","지금 재질 최적화",       "Optimizar materiales ahora"}},
        {"tools.anim.section",   new[]{"🎬 ANIMATOR",                "🎬 ANIMATOR",              "🎬 アニメーター",         "🎬 애니메이터",          "🎬 ANIMATOR"}},
        {"tools.anim.opt",       new[]{"Optimiser Animator",         "Optimize Animator",        "Animatorを最適化",        "애니메이터 최적화",      "Optimizar Animator"}},
        {"tools.anim.params",    new[]{"Params inutilises",          "Unused params",            "未使用パラメータ",        "미사용 파라미터",        "Params sin usar"}},
        {"tools.anim.layers",    new[]{"Layers vides",               "Empty layers",             "空のレイヤー",            "빈 레이어",              "Capas vacías"}},
        {"tools.anim.keyframes", new[]{"Keyframes redondants",       "Redundant keyframes",      "冗長なキーフレーム",      "중복 키프레임",          "Keyframes redundantes"}},
        {"tools.shaders.section",new[]{"🎨 FIX SHADERS (QUEST COMPATIBILITY)","🎨 FIX SHADERS (QUEST COMPATIBILITY)","🎨 シェーダー修正（Quest互換）","🎨 쉐이더 수정 (Quest 호환)","🎨 FIX SHADERS (COMPATIBILIDAD QUEST)"}},
        {"tools.shaders.fix",    new[]{"Fix shaders Quest",          "Fix Quest shaders",        "Questシェーダー修正",     "Quest 쉐이더 수정",      "Fix shaders Quest"}},
        {"tools.shaders.replace",new[]{"Remplacer par",              "Replace with",             "置き換え先",              "교체할 대상",            "Reemplazar por"}},
        {"tools.shaders.scan",   new[]{"Scanner shaders non-Quest",  "Scan non-Quest shaders",   "非QuestシェーダーをスキャN","비Quest 쉐이더 스캔",    "Escanear shaders no-Quest"}},
        {"tools.lod.section",    new[]{"📉 LOD GROUP",               "📉 LOD GROUP",             "📉 LODグループ",          "📉 LOD 그룹",            "📉 LOD GROUP"}},
        {"tools.lod.gen",        new[]{"Generer LOD Group",          "Generate LOD Group",       "LODグループ生成",         "LOD 그룹 생성",          "Generar LOD Group"}},
        {"tools.run.btn",        new[]{"🚀 Lancer l'optimisation",   "🚀 Run Optimization",      "🚀 最適化を実行",         "🚀 최적화 실행",         "🚀 Ejecutar optimización"}},
        {"tools.run.confirm",    new[]{"Lancer l'optimisation ?\nCtrl+Z pour annuler.",
                                       "Run optimization?\nCtrl+Z to undo.",
                                       "最適化を実行しますか？\nCtrl+Zで元に戻せます。",
                                       "최적화를 실행하시겠습니까?\nCtrl+Z로 되돌릴 수 있습니다.",
                                       "¿Ejecutar optimización?\nCtrl+Z para deshacer."}},
        // ─ Presets ─────────────────────────────────────────────────
        {"presets.create.section",new[]{"➕ CREER UN PRESET",        "➕ CREATE PRESET",         "➕ プリセット作成",        "➕ 프리셋 만들기",        "➕ CREAR PRESET"}},
        {"presets.include",      new[]{"Inclure dans ce preset :",   "Include in preset:",       "このプリセットに含める:",  "프리셋에 포함:",         "Incluir en este preset:"}},
        {"presets.save.btn",     new[]{"💾 Sauvegarder preset",      "💾 Save Preset",           "💾 プリセット保存",        "💾 프리셋 저장",          "💾 Guardar preset"}},
        {"presets.import.btn",   new[]{"📥 Import JSON",             "📥 Import JSON",           "📥 JSONインポート",        "📥 JSON 가져오기",        "📥 Importar JSON"}},
        {"presets.saved.section",new[]{"💾 PRESETS SAUVEGARDES",     "💾 SAVED PRESETS",         "💾 保存済みプリセット",    "💾 저장된 프리셋",        "💾 PRESETS GUARDADOS"}},
        {"presets.none",         new[]{"Aucun preset.",              "No presets.",              "プリセットなし。",         "프리셋 없음.",            "Sin presets."}},
        {"presets.load.btn",     new[]{"Charger",                    "Load",                     "ロード",                  "불러오기",               "Cargar"}},
        {"presets.applied.btn",  new[]{"Applique",                   "Applied",                  "適用済み",                "적용됨",                 "Aplicado"}},
        {"presets.select.btn",   new[]{"Selectionner",               "Select",                   "選択",                    "선택",                   "Seleccionar"}},
        // ─ BlendShapes ─────────────────────────────────────────────
        {"bs.section",           new[]{"🎭 GESTION DES BLENDSHAPES", "🎭 BLENDSHAPE MANAGEMENT", "🎭 ブレンドシェイプ管理",  "🎭 블렌드쉐이프 관리",   "🎭 GESTIÓN DE BLENDSHAPES"}},
        {"bs.info",              new[]{"Vert = poids actif (>0%), gris = poids zero. Decocher = sera supprime a l'application.",
                                       "Green = active weight (>0%), grey = zero weight. Unchecked = will be deleted on apply.",
                                       "緑=アクティブ重み(>0%)、グレー=ゼロ重み。チェック外=適用時削除。",
                                       "녹색=활성 가중치(>0%), 회색=0 가중치. 해제=적용 시 삭제.",
                                       "Verde = peso activo (>0%), gris = peso cero. Sin marcar = se eliminará al aplicar."}},
        {"bs.none.found",        new[]{"Aucun blendshape trouve.",   "No blendshapes found.",    "ブレンドシェイプなし。",   "블렌드쉐이프 없음.",     "No se encontraron blendshapes."}},
        {"bs.auto.section",      new[]{"⚡ AUTO-OPTIMISATION",       "⚡ AUTO-OPTIMIZATION",     "⚡ 自動最適化",            "⚡ 자동 최적화",          "⚡ AUTO-OPTIMIZACIÓN"}},
        {"bs.auto.info",         new[]{"Applique un preset en un clic — cocher/decocher les blendshapes automatiquement :",
                                       "Apply a preset in one click — automatically check/uncheck blendshapes:",
                                       "1クリックでプリセット適用 — ブレンドシェイプを自動でチェック/解除:",
                                       "한 번의 클릭으로 프리셋 적용 — 자동으로 블렌드쉐이프 체크/해제:",
                                       "Aplica un preset con un clic — marca/desmarca blendshapes automáticamente:"}},
        {"bs.search",            new[]{"Rechercher :",               "Search:",                  "検索:",                   "검색:",                  "Buscar:"}},
        {"bs.uncheck.nonviseme", new[]{"Decocher non-viseme",        "Uncheck non-viseme",       "非ビゼムを解除",           "비-비세임 해제",          "Desmarcar no-viseme"}},
        {"bs.by.mesh",           new[]{"📊 PAR MESH",                "📊 BY MESH",               "📊 メッシュ別",            "📊 메시별",               "📊 POR MESH"}},
        {"bs.kept.label",        new[]{"gardes",                     "kept",                     "保持",                    "유지",                   "conservados"}},
        {"bs.preview.label",     new[]{"Apercu : ",                  "Preview: ",                "プレビュー: ",             "미리보기: ",              "Vista previa: "}},
        {"bs.kept.short",        new[]{"gardés",                     "kept",                     "保持",                    "유지",                   "conservados"}},
        {"bs.removed.short",     new[]{"supprimés",                  "removed",                  "削除",                    "삭제",                   "eliminados"}},
        // ─ Log ─────────────────────────────────────────────────────
        {"log.section",          new[]{"📝 JOURNAL DES OPERATIONS",  "📝 OPERATION LOG",         "📝 操作ログ",              "📝 작업 로그",            "📝 REGISTRO DE OPERACIONES"}},
        {"log.errors",           new[]{"Erreurs",                    "Errors",                   "エラー",                  "오류",                   "Errores"}},
        {"log.warnings",         new[]{"Warnings",                   "Warnings",                 "警告",                    "경고",                   "Advertencias"}},
        {"log.info.tab",         new[]{"Info",                       "Info",                     "情報",                    "정보",                   "Info"}},
        {"log.success",          new[]{"Succes",                     "Success",                  "成功",                    "성공",                   "Éxito"}},
        {"log.export.btn",       new[]{"📤 Exporter .txt",           "📤 Export .txt",           "📤 .txtをエクスポート",    "📤 .txt 내보내기",        "📤 Exportar .txt"}},
        {"log.clear.btn",        new[]{"🗑️ Vider",                   "🗑️ Clear",                "🗑️ クリア",               "🗑️ 비우기",              "🗑️ Vaciar"}},
        {"log.empty",            new[]{"Aucune operation effectuee.","No operations performed.",  "操作が行われていません。",  "수행된 작업이 없습니다.", "No se realizaron operaciones."}},
    };

    private string T(string key)
    {
        if (LOC.TryGetValue(key, out var arr) && _lang < arr.Length) return arr[_lang];
        return key;
    }

    // ═════════════════════════════════════════════════════════════
    //  NAVIGATION
    // ═════════════════════════════════════════════════════════════
    private int      _tab = 0;
    private string[] _tabIcons  = { "Search Icon","d_PhysicsManager Icon","AvatarSelector","BlendTree Icon","MeshFilter Icon","Texture Icon","SettingsIcon","UnityEditor.ConsoleWindow" };
    private Vector2  _scroll;
    private GameObject _avatar;

    // ═════════════════════════════════════════════════════════════
    //  ANALYSE
    // ═════════════════════════════════════════════════════════════
    private class AnalyseResults
    {
        public int totalObjects, disabledObjects, missingScripts, emptyObjects;
        public int totalPolygons, totalMaterials, totalBones;
        public int totalPhysBones, totalPhysBoneColliders;
        public int totalBlendShapes, lightCount, particleCount, audioSourceCount, cameraCount;
        public int globalRank;
        public int projectedRank;
        public int questRank;
        public int pcRank;
        public int questProjectedRank;
        public int pcProjectedRank;
    }
    private AnalyseResults _results;
    private bool _hasAnalysed;

    // Limites VRChat
    private const int LIM_PB = 8, LIM_TRSF = 64, LIM_COL = 16, LIM_CC = 64, LIM_CONT = 16;

    // ═════════════════════════════════════════════════════════════
    //  PHYSICS
    // ═════════════════════════════════════════════════════════════
    private class PhysEntry { public Component component; public string boneName; public bool isCollider; public bool keep = true; public int childCount; }
    private List<PhysEntry> _phys = new List<PhysEntry>();
    private bool _physScanned; private Vector2 _physScrollPB, _physScrollCol;
    private int _physKeepTopN = 8;

    // ═════════════════════════════════════════════════════════════
    //  BONES
    // ═════════════════════════════════════════════════════════════
    private enum BoneUsage { Skinned, PhysBone, Structural, Unused }
    private class BoneEntry
    {
        public Transform bone;
        public string    shortName;
        public string    fullPath;
        public int       depth;
        public bool      keep = true;
        public BoneUsage usage;
        public bool      hasPhysBone;
        public int       verticesInfluenced;
        public SkinnedMeshRenderer ownerSMR;
    }
    private List<BoneEntry> _boneList = new List<BoneEntry>();
    private bool _bonesScanned; private Vector2 _boneScroll;
    private string _boneSearch = "";
    private int _boneFilter = 0;
    private static readonly string[] _boneFilterLabels = { "Tous","Skinnes","PhysBones","Structurels","Non utilises" };
    private SkinnedMeshRenderer _boneMeshFilter;

    // ═════════════════════════════════════════════════════════════
    //  MESH + UV PAINTER
    // ═════════════════════════════════════════════════════════════
    private int  _meshQuality    = 75, _maxTrisQuest = 32000, _maxTrisPC = 70000, _blendMode = 2;
    private int  _compressionLvl = 1; // 0=Off 1=Low 2=Medium 3=High
    private static readonly string[] _compressionLvlLabels = { "Off", "Low", "Medium", "High" };
    private bool _meshCopyBeforeCompression = true;
    private enum UVPaintMode { Brush, Rectangle, Lasso }
    private UVPaintMode _uvMode = UVPaintMode.Brush;
    private bool _uvPaintActive;
    private SkinnedMeshRenderer _uvTargetSMR;
    private Texture2D _uvPreviewTex, _uvMaskTex;
    private int _uvBrushSize = 20;
    private float _uvBrushOpacity = 1f;
    private Vector2 _uvScroll;
    private List<int> _markedTriangles = new List<int>();
    private Stack<HashSet<int>> _uvUndoStack = new Stack<HashSet<int>>();
    private Stack<HashSet<int>> _uvRedoStack = new Stack<HashSet<int>>();
    private Vector2? _rectStart;
    private List<Vector2> _lassoPoints = new List<Vector2>();
    private const int UV_TEX_SIZE = 512;
    private List<SkinnedMeshRenderer> _meshMergeTargets = new List<SkinnedMeshRenderer>();
    private List<int> _hiddenTriangles = new List<int>();
    private bool _show3DPreview = false;
    private PreviewRenderUtility _previewUtility;
    private float _preview3DRotY = 0f;

    private static readonly string[] _blendModes = {
        "Garder tous","Garder viseme + expressions","Garder viseme uniquement","Supprimer tous"
    };
    private static readonly string[] _visemeNames = {
        "vrc.v_sil","vrc.v_pp","vrc.v_ff","vrc.v_th","vrc.v_dd","vrc.v_kk","vrc.v_ch",
        "vrc.v_ss","vrc.v_nn","vrc.v_rr","vrc.v_aa","vrc.v_e","vrc.v_ih","vrc.v_oh","vrc.v_ou"
    };
    private static readonly string[] _exprNames = { "happy","sad","angry","surprised","blink","blink_l","blink_r" };

    // ═════════════════════════════════════════════════════════════
    //  TEXTURES
    // ═════════════════════════════════════════════════════════════
    private static readonly int[]    _maxSizeVals   = { 32,64,128,256,512,1024,2048,4096,8192 };
    private static readonly string[] _maxSizeLabels = { "32","64","128","256","512","1024","2048","4096","8192" };
    private int _texMaxSizeIdx = 5, _texResizeAlgo = 0, _texFormatIdx = 0, _texCompIdx = 3;
    private bool _texUseCrunch; private int _texCrunchQuality = 50;
    private int  _texPlatform = 0; private bool _texGenMipmaps = true, _texSRGB = true;

    private class TexEntry { public Texture texture; public string path; public int curSize; public int targetSize; public bool overridden; public int overrideSize; public bool overrideCompression; public int overrideCompIdx; public bool keep = true; }
    private List<TexEntry> _texList = new List<TexEntry>();
    private bool _texScanned; private Vector2 _texScroll;
    private List<Texture> _unusedTextures = new List<Texture>();

    private static readonly string[] _resizeAlgos = { "Mitchell","Bilinear" };
    private static readonly string[] _platformLabels = { "Quest (Android)","PC (Standalone)","Quest + PC" };
    private static readonly string[] _compLabels = { "None","Low Quality","Normal Quality","High Quality" };
    private static readonly TextureImporterCompression[] _compVals = {
        TextureImporterCompression.Uncompressed, TextureImporterCompression.CompressedLQ,
        TextureImporterCompression.Compressed,   TextureImporterCompression.CompressedHQ
    };
    private static readonly string[] _fmtLabels = {
        "Automatic","RGB Compressed DXT1","RGBA Compressed DXT5",
        "RGB Compressed BC6H","RGBA Compressed BC7","RGBA Compressed EAC",
        "RGB 16 bit","RGBA 32 bit","ASTC 4x4","ASTC 6x6","ASTC 8x8","ETC2 RGBA8"
    };
    private static readonly TextureImporterFormat[] _fmtVals = {
        TextureImporterFormat.Automatic, TextureImporterFormat.DXT1, TextureImporterFormat.DXT5,
        TextureImporterFormat.BC6H, TextureImporterFormat.BC7, TextureImporterFormat.EAC_RG,
        TextureImporterFormat.RGB16, TextureImporterFormat.RGBA32,
        TextureImporterFormat.ASTC_4x4, TextureImporterFormat.ASTC_6x6,
        TextureImporterFormat.ASTC_8x8, TextureImporterFormat.ETC2_RGBA8
    };

    // ═════════════════════════════════════════════════════════════
    //  OUTILS
    // ═════════════════════════════════════════════════════════════
    private bool _opt_missingScripts = true, _opt_emptyObjects = false, _opt_dupMaterials = true;
    private bool _opt_removeAudio = false, _opt_removeCameras = false;
    private int _selectedQuickPreset = 0;
    private bool _opt_animator = false, _opt_unusedParams = true, _opt_emptyLayers = true, _opt_cleanKeyframes = false;
    private bool _opt_lodGroup = false; private float _opt_lodDist1 = 0.5f, _opt_lodDist2 = 0.15f;
    private bool _opt_fixShaders = false; private int _opt_targetShader = 0;
    private int _opt_blendMode = 1;
    private bool _opt_backup = true; private string _opt_backupPath = "Assets/Backup/";

    // ─── MODE COPIE ───────────────────────────────────────────
    // Désactivé et ignoré : toutes les modifications s'appliquent à l'avatar sélectionné.
    private bool       _dupMode       = false;
    private string     _dupFolder     = "Assets/Optimized/";
    private GameObject _workingCopy   = null;
    private string     _workingPrefabPath = "";

    private string _questOutputPath = "Assets/Quest_Optimized/";
    private bool   _questRemoveLights    = true;
    private bool   _questRemoveParticles = true;
    private bool   _questRemoveAudio     = true;
    private bool   _questRemoveCameras   = true;
    private bool   _questFixShaders      = true;
    private bool   _questRemoveMissing   = true;
    private int    _questTargetShader    = 0; // VRChat/Mobile/Toon Lit
    private static readonly string[] _targetShaderNames = { "VRChat/Mobile/Toon Lit","VRChat/Mobile/Standard Lite","VRChat/Mobile/Particles/Additive" };

    private class MatFixEntry { public Material mat; public string currentShader; public bool fix = true; }
    private List<MatFixEntry> _matFixList = new List<MatFixEntry>();
    private bool _matFixScanned;

    private class MatScanEntry { public Material mat; public string shaderName; public List<string> meshes = new List<string>(); public int slotCount; }
    private List<MatScanEntry> _matScanList = new List<MatScanEntry>();
    private bool _matScanned;
    private Vector2 _matScroll;
    private Vector2 _matListScroll;
    private string  _matListSearch    = "";
    private bool    _matListExpanded  = true;
    private bool _matTexApply = true;
    private int  _matTexMaxSizeIdx = 5; // 1024
    private int  _matTexCompIdx    = 2; // Normal Quality
    private int  _matTexPlatform   = 0; // Quest

    // ─── BLENDSHAPES ──────────────────────────────────────────────
    private class BSEntry { public SkinnedMeshRenderer smr; public string name; public int index; public float weight; public bool keep = true; }
    private List<BSEntry> _bsList = new List<BSEntry>();
    private bool _bsScanned; private Vector2 _bsScroll; private Vector2 _bsMeshScroll;
    private string _bsSearch = ""; private int _bsFilter = 0;
    private Dictionary<string,bool> _bsMeshFoldout = new Dictionary<string,bool>();

    // ═════════════════════════════════════════════════════════════
    //  PRESETS
    // ═════════════════════════════════════════════════════════════
    [Serializable]
    public class OptimizerPreset
    {
        public string name = "Nouveau preset";
        public bool includeMesh = true, includeTextures = true, includePhysics = true, includeTools = true;

        public int meshQuality = 75, maxTrisQuest = 32000, maxTrisPC = 70000, blendMode = 2;
        public int texMaxSizeIdx = 5, texFormatIdx = 0, texCompIdx = 3;
        public bool texUseCrunch, texSRGB = true, texGenMipmaps = true;
        public int texPlatform = 0, texCrunchQuality = 50;
        public int physKeepTopN = 8;
        public bool optMissingScripts = true, optEmptyObjects = true, optDupMaterials = true;
        public bool optRemoveAudio, optRemoveCameras, optFixShaders;
        public int optTargetShader = 0;
    }

    private string _presetName = "Mon preset";
    private bool _presetIncMesh = true, _presetIncTex = true, _presetIncPhys = true, _presetIncTools = true;
    private List<OptimizerPreset> _presets = new List<OptimizerPreset>();
    private Vector2 _presetScroll;
    private const string PRESETS_FILE = "Assets/Editor/VRCOptimizerPresets.json";

    // ═════════════════════════════════════════════════════════════
    //  LOG  (ameliore)
    // ═════════════════════════════════════════════════════════════
    public enum LogLevel { Info, Success, Warn, Error }
    [Serializable]
    public class LogEntry
    {
        public DateTime time;
        public LogLevel level;
        public string   message;
        public int      objectId;  // instanceID de l'objet lie (optional)
    }
    private List<LogEntry> _log = new List<LogEntry>();
    private bool _logShowInfo = true, _logShowSuccess = true, _logShowWarn = true, _logShowError = true;

    // ═════════════════════════════════════════════════════════════
    //  STYLES
    // ═════════════════════════════════════════════════════════════
    private GUIStyle _styleCard, _styleSectionLabel, _styleAccentBtn, _styleDangerBtn,
                     _styleUndoBtn, _styleSuccessBtn, _styleSmall, _styleLink;
    private bool _stylesInit;

    // ═════════════════════════════════════════════════════════════
    //  MENU + LIFECYCLE
    // ═════════════════════════════════════════════════════════════
    [MenuItem("Tools/NETRA Avatar Optimizer")]
    public static void Open()
    {
        var w = GetWindow<VRChatAvatarOptimizer>("NETRA Avatar Optimizer");
        w.minSize = new Vector2(380, 560);
    }

    private void OnEnable()  { LoadPresets(); SceneView.duringSceneGui += OnSceneGUI; }
    private void OnDisable() { SceneView.duringSceneGui -= OnSceneGUI; if (_previewUtility != null) { _previewUtility.Cleanup(); _previewUtility = null; } }

    private void OnSceneGUI(SceneView sv)
    {
        if (_uvTargetSMR == null || _uvTargetSMR.sharedMesh == null || _markedTriangles.Count == 0) return;
        var mesh = _uvTargetSMR.sharedMesh;
        var verts = mesh.vertices;
        var tris  = mesh.triangles;
        var tf    = _uvTargetSMR.transform;
        Handles.color = new Color(1f, 0.9f, 0f, 0.45f);
        foreach (int triIdx in _markedTriangles)
        {
            int i = triIdx * 3;
            if (i + 2 >= tris.Length) continue;
            Handles.DrawAAConvexPolygon(
                tf.TransformPoint(verts[tris[i]]),
                tf.TransformPoint(verts[tris[i+1]]),
                tf.TransformPoint(verts[tris[i+2]]));
        }
        sv.Repaint();
    }

    private void InitStyles()
    {
        if (_stylesInit) return;
        _stylesInit = true;

        _styleCard = new GUIStyle(EditorStyles.helpBox)
        {
            normal  = { background = MakeTex(1,1,COL_BG3) },
            border  = new RectOffset(1,1,1,1),
            padding = new RectOffset(12,12,10,10),
            margin  = new RectOffset(0,0,4,4)
        };
        _styleSectionLabel = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            normal   = { textColor = COL_ACCENT2 },
            padding  = new RectOffset(2,2,6,2)
        };
        _styleAccentBtn = new GUIStyle(GUI.skin.button)
        {
            normal    = { background = MakeTex(1,1,COL_ACCENT),  textColor = Color.white },
            hover     = { background = MakeTex(1,1,COL_ACCENT2), textColor = Color.white },
            active    = { background = MakeTex(1,1,COL_BG3),     textColor = Color.white },
            fontStyle = FontStyle.Bold, fontSize = 12,
            padding   = new RectOffset(10,10,6,6)
        };
        _styleSuccessBtn = new GUIStyle(GUI.skin.button)
        {
            normal    = { background = MakeTex(1,1, new Color(0.25f,0.80f,0.45f)), textColor = Color.white },
            hover     = { background = MakeTex(1,1, new Color(0.30f,0.90f,0.58f)), textColor = Color.white },
            active    = { background = MakeTex(1,1, new Color(0.20f,0.65f,0.35f)), textColor = Color.white },
            fontStyle = FontStyle.Bold, fontSize = 12,
            padding   = new RectOffset(10,10,6,6)
        };
        _styleDangerBtn = new GUIStyle(GUI.skin.button)
        {
            normal    = { background = MakeTex(1,1,new Color(0.68f,0.22f,0.28f)), textColor = Color.white },
            hover     = { background = MakeTex(1,1,new Color(0.84f,0.30f,0.34f)), textColor = Color.white },
            active    = { background = MakeTex(1,1,new Color(0.55f,0.15f,0.20f)), textColor = Color.white },
            fontStyle = FontStyle.Bold, fontSize = 12,
            padding   = new RectOffset(10,10,6,6)
        };
        _styleUndoBtn = new GUIStyle(GUI.skin.button)
        {
            normal    = { background = MakeTex(1,1,COL_BG2), textColor = COL_TEXT },
            hover     = { background = MakeTex(1,1,new Color(0.20f,0.27f,0.35f)), textColor = COL_TEXT },
            fontStyle = FontStyle.Bold, fontSize = 11,
            padding   = new RectOffset(8,8,4,4)
        };
        _styleLink = new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = COL_ACCENT2 },
            hover     = { textColor = Color.white },
            fontStyle = FontStyle.Bold, fontSize = 11
        };
        _styleSmall = new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = COL_TEXT_DIM },
            fontSize = 10
        };
    }

    private Texture2D MakeTex(int w, int h, Color col)
    {
        var t = new Texture2D(w,h);
        var px = new Color[w*h];
        for (int i = 0; i < px.Length; i++) px[i] = col;
        t.SetPixels(px); t.Apply();
        return t;
    }

    // ═════════════════════════════════════════════════════════════
    //  OnGUI
    // ═════════════════════════════════════════════════════════════
    private void OnGUI()
    {
        InitStyles();
        EditorGUI.DrawRect(new Rect(0,0,position.width,position.height), COL_BG);

        DrawTopBar();
        DrawAvatarField();
        DrawTabBar();

        if (_tab >= 9) _tab = 0;

        _scroll = EditorGUILayout.BeginScrollView(_scroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.scrollView);
        EditorGUILayout.Space(4);
        switch (_tab)
        {
            case 0: DrawAnalyse();       break;
            case 1: DrawPhysics();       break;
            case 2: DrawBones();         break;
            case 3: DrawBlendshapeTab(); break;
            case 4: DrawMeshUV();        break;
            case 5: DrawTextures();      break;
            case 6: DrawMaterials();     break;
            case 7: DrawOutils();        break;
            case 8: DrawLog();           break;
        }
        EditorGUILayout.Space(12);
        EditorGUILayout.EndScrollView();
    }

    // ─── TOP BAR ─────────────────────────────────────────────
    private void DrawTopBar()
    {
        EditorGUI.DrawRect(new Rect(0,0,position.width,44), COL_BG);
        EditorGUI.DrawRect(new Rect(0,43,position.width,1), COL_BG2);

        bool compact = position.width < 460;

        EditorGUI.DrawRect(new Rect(10,8,28,28), COL_BG2);
        EditorGUI.DrawRect(new Rect(12,10,24,24), COL_BG3);

        // Undo button (right)
        float undoW  = compact ? 28 : 114;
        float undoX  = position.width - undoW - 8;
        var undoStyle = new GUIStyle(GUI.skin.button)
        {
            normal    = { background = MakeTex(1,1,COL_BG2), textColor = COL_TEXT_DIM },
            hover     = { background = MakeTex(1,1,new Color(0.20f,0.26f,0.34f)), textColor = COL_TEXT },
            active    = { background = MakeTex(1,1,COL_BG3), textColor = COL_TEXT },
            fontStyle = FontStyle.Bold, fontSize = 11
        };
        string undoLabel = compact ? "↶" : T("btn.undo");
        if (GUI.Button(new Rect(undoX, 12, undoW, 22), new GUIContent(undoLabel), undoStyle))
        { Undo.PerformUndo(); Log(LogLevel.Info, "Undo"); }

        // Lang dropdown (right, just left of undo)
        float ddW   = 52f;
        float ddX   = undoX - ddW - 6;
        bool showDD = ddX > 120;

        if (showDD)
        {
            var ddStyle = new GUIStyle(EditorStyles.popup)
            {
                normal   = { background = MakeTex(1,1,COL_BG2), textColor = COL_TEXT },
                hover    = { background = MakeTex(1,1,COL_BG3), textColor = Color.white },
                focused  = { background = MakeTex(1,1,COL_BG3), textColor = Color.white },
                fontSize = 10, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding  = new RectOffset(4,4,0,0)
            };
            _lang = EditorGUI.Popup(new Rect(ddX, 13, ddW, 20), _lang, LANG_CODES, ddStyle);
        }

        // Title (left)
        float titleX = 46;
        float titleW = showDD ? ddX - titleX - 4 : undoX - titleX - 4;
        string titleTxt = compact ? T("app.title.short") : T("app.title");
        GUI.Label(new Rect(titleX, 8, Mathf.Max(titleW, 60), 28),
            new GUIContent(titleTxt, EditorGUIUtility.IconContent("AvatarSelector").image),
            new GUIStyle(EditorStyles.boldLabel)
            { fontSize = compact ? 12 : 15, normal = { textColor = COL_TEXT }, alignment = TextAnchor.MiddleLeft });

        GUILayout.Space(48);
    }

    private void DrawAvatarField()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(8);
        GUILayout.Label(new GUIContent(T("avatar.label"), T("avatar.tooltip")),
            new GUIStyle(EditorStyles.label)
            { normal = { textColor = COL_TEXT_DIM }, fontStyle = FontStyle.Bold, fontSize = 12 }, GUILayout.Width(60));
        EditorGUI.BeginChangeCheck();
        _avatar = (GameObject)EditorGUILayout.ObjectField(_avatar, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck())
        {
            _hasAnalysed = _physScanned = _bonesScanned = _texScanned = _matFixScanned = _bsScanned = _matScanned = false;
            _results = null; _phys.Clear(); _boneList.Clear(); _texList.Clear(); _bsList.Clear();
            _uvTargetSMR = null; _uvPreviewTex = null; _uvMaskTex = null; _markedTriangles.Clear();
            _workingCopy = null; _workingPrefabPath = "";
        }
        GUILayout.Space(8);
        EditorGUILayout.EndHorizontal();

        // ── Mode copie ──────────────────────────────────────
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(8);
        GUI.enabled = false;
        GUILayout.Toggle(false,
            new GUIContent(T("copy.mode"), T("copy.mode.tooltip")),
            GUILayout.ExpandWidth(false));
        GUI.enabled = true;
        GUILayout.FlexibleSpace();
        GUILayout.Space(8);
        EditorGUILayout.EndHorizontal();

        if (_dupMode)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(8);
            GUILayout.Label(T("lbl.folder"), new GUIStyle(EditorStyles.miniLabel){ normal={textColor=COL_TEXT_DIM}, fixedWidth=52 });
            _dupFolder = EditorGUILayout.TextField(_dupFolder,
                new GUIStyle(EditorStyles.textField){ fontSize=10 });

            if (_workingCopy != null)
            {
                GUILayout.Space(4);
                var statusStyle = new GUIStyle(EditorStyles.miniLabel){ normal={textColor=COL_SUCCESS}, fontStyle=FontStyle.Bold };
                GUILayout.Label("✔ " + _workingCopy.name, statusStyle, GUILayout.ExpandWidth(false));
                if (GUILayout.Button("×", new GUIStyle(EditorStyles.miniButton){ normal={textColor=COL_ERROR} }, GUILayout.Width(18)))
                { _workingCopy = null; _workingPrefabPath = ""; }
            }
            GUILayout.Space(8);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(4);
        DrawSep();
    }

    private void DrawTabBar()
    {
        // On narrow windows split into 2 rows of tabs
        bool twoRows = position.width < 580;
        const int TAB_COUNT = 9;
        int  rowSplit = twoRows ? 5 : TAB_COUNT;

        for (int row = 0; row < (twoRows ? 2 : 1); row++)
        {
            int start = row * rowSplit;
            int end   = Mathf.Min(start + rowSplit, TAB_COUNT);

            EditorGUILayout.BeginHorizontal(GUILayout.Height(twoRows ? 28 : 34));
            GUILayout.Space(4);
            for (int i = start; i < end; i++)
            {
                bool active = _tab == i;
                var s = new GUIStyle(GUI.skin.button)
                {
                    normal    = { background = MakeTex(1,1, active ? COL_BG3 : COL_BG2), textColor = active ? Color.white : COL_TEXT_DIM },
                    hover     = { background = MakeTex(1,1, new Color(0.24f,0.28f,0.34f)), textColor = COL_TEXT },
                    active    = { background = MakeTex(1,1, COL_BG3), textColor = Color.white },
                    border    = new RectOffset(1,1,1,1),
                    fontStyle = active ? FontStyle.Bold : FontStyle.Normal,
                    fontSize  = twoRows ? 10 : 11,
                    padding   = new RectOffset(twoRows ? 6 : 10, twoRows ? 6 : 8, 4, 4),
                    margin    = new RectOffset(2,2, twoRows ? 2 : 4, twoRows ? 2 : 4),
                    alignment = TextAnchor.MiddleCenter
                };
                var content = new GUIContent(T("tab." + i));
                if (GUILayout.Button(content, s)) _tab = i;

                Color dotCol = GetTabStatusColor(i);
                if (dotCol.a > 0.01f)
                {
                    var br = GUILayoutUtility.GetLastRect();
                    DrawStatusDot(new Rect(br.xMax - 11, br.y + 4, 8, 8), dotCol);
                }
            }
            GUILayout.Space(4);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space(4);
    }

    private void DrawStatusDot(Rect r, Color col)
    {
        // Cercle approximé : carré central + bandes horizontales/verticales réduites
        EditorGUI.DrawRect(new Rect(r.x+1, r.y,   r.width-2, r.height),   col);
        EditorGUI.DrawRect(new Rect(r.x,   r.y+1, r.width,   r.height-2), col);
    }

    private Color GetTabStatusColor(int tab)
    {
        if (!_hasAnalysed || _results == null) return Color.clear;
        var r = _results;
        switch (tab)
        {
            case 0: // Analyse
                return r.globalRank <= 1 ? COL_SUCCESS : r.globalRank <= 2 ? COL_WARN : COL_ERROR;
            case 1: // Physics
                return r.totalPhysBones == 0 ? COL_SUCCESS
                     : r.totalPhysBones <= 4  ? COL_SUCCESS
                     : r.totalPhysBones <= 16 ? COL_WARN : COL_ERROR;
            case 2: // Bones
                return r.totalBones <= 90  ? COL_SUCCESS
                     : r.totalBones <= 150 ? COL_WARN : COL_ERROR;
            case 3: // BlendShapes
                return r.totalBlendShapes <= 7  ? COL_SUCCESS
                     : r.totalBlendShapes <= 52 ? COL_WARN : COL_ERROR;
            case 4: // Mesh & UV
                return r.totalPolygons <= 10000 ? COL_SUCCESS
                     : r.totalPolygons <= 20000 ? COL_WARN : COL_ERROR;
            case 5: // Textures
                if (!_texScanned) return Color.clear;
                return _texList.Any(t => t.curSize > 2048) ? COL_ERROR
                     : _texList.Any(t => t.curSize > 1024) ? COL_WARN : COL_SUCCESS;
            case 6: // Matériaux
                if (!_matScanned) return Color.clear;
                int nonQ = _matScanList.Count(e => !e.shaderName.StartsWith("VRChat/Mobile/") && !e.shaderName.Contains("Mobile"));
                return nonQ == 0 ? COL_SUCCESS : nonQ <= 3 ? COL_WARN : COL_ERROR;
            default: return Color.clear;
        }
    }

    // ═════════════════════════════════════════════════════════════
    //  TAB 0 — ANALYSE (avec jauge circulaire + rang projete + export)
    // ═════════════════════════════════════════════════════════════
    private void DrawAnalyse()
    {
        SectionLabel(T("scan.section"));
        bool narrowScan = position.width < 600;
        if (narrowScan)
        {
            if (AccentBtn(new GUIContent(T("scan.btn")), GUILayout.Height(32), GUILayout.ExpandWidth(true)))
            {
                if (_avatar == null) EditorUtility.DisplayDialog("VRC Optimizer", T("msg.no.avatar"), T("dlg.ok"));
                else RunAnalysis();
            }
            if (_hasAnalysed)
            {
                EditorGUILayout.BeginHorizontal();
                if (SuccessBtn(new GUIContent(T("scan.all.short")), GUILayout.Height(28), GUILayout.ExpandWidth(true)))
                {
                    if (EditorUtility.DisplayDialog(T("dlg.optim.title"), T("dlg.optim.msg"), T("dlg.apply.all"), T("dlg.cancel")))
                        RunFullOptimization();
                }
                if (UndoBtn(new GUIContent("↶"), GUILayout.Height(28), GUILayout.Width(32))) Undo.PerformUndo();
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            if (AccentBtn(new GUIContent(T("scan.btn")), GUILayout.Height(36), GUILayout.ExpandWidth(true)))
            {
                if (_avatar == null) EditorUtility.DisplayDialog("VRC Optimizer", T("msg.no.avatar"), T("dlg.ok"));
                else RunAnalysis();
            }
            if (_hasAnalysed && SuccessBtn(new GUIContent(T("scan.all")), GUILayout.Height(36), GUILayout.Width(200)))
            {
                if (EditorUtility.DisplayDialog(T("dlg.optim.title"), T("dlg.optim.msg"), T("dlg.apply.all"), T("dlg.cancel")))
                    RunFullOptimization();
            }
            if (_hasAnalysed && UndoBtn(new GUIContent(T("btn.undo")), GUILayout.Height(36), GUILayout.Width(100))) Undo.PerformUndo();
            EditorGUILayout.EndHorizontal();
        }

        if (!_hasAnalysed || _results == null) return;

        EditorGUILayout.Space(8);
        var r = _results;

        // ── SCORE VRCHAT ─────────────────────────────────────
        SectionLabel(T("score.title"));
        BeginCard();

        float globalPct = CalcGlobalScorePct(r);
        var questPct    = CalcPlatformScorePct(r, true);
        var pcPct       = CalcPlatformScorePct(r, false);
        int curRank     = Mathf.Clamp(r.globalRank,    0, 4);
        int projRank    = Mathf.Clamp(r.projectedRank, 0, 4);

        // ── ROW 1 : jauge principale + rang ──────────────────
        bool narrowScore = position.width < 420;
        int gaugeSize = narrowScore ? 90 : 130;
        if (narrowScore) EditorGUILayout.BeginVertical(); else EditorGUILayout.BeginHorizontal();

        // Jauge circulaire
        if (narrowScore)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
        }
        var gaugeRect = GUILayoutUtility.GetRect(gaugeSize, gaugeSize, GUILayout.Width(gaugeSize), GUILayout.Height(gaugeSize));
        DrawCircularGauge(gaugeRect, globalPct);
        if (narrowScore)
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(narrowScore ? 4 : 16);
        EditorGUILayout.BeginVertical();
        GUILayout.Space(10);

        // Badge rang actuel
        GUILayout.Label(T("score.rankCur"), new GUIStyle(EditorStyles.miniLabel)
            { normal={ textColor=COL_TEXT_DIM }, fontStyle=FontStyle.Bold, fontSize=9 });
        GUILayout.Space(2);
        var badgeR = GUILayoutUtility.GetRect(0, 32, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(new Rect(badgeR.x, badgeR.y, 4, badgeR.height), RANK_COLS[curRank]);
        EditorGUI.DrawRect(new Rect(badgeR.x+4, badgeR.y, badgeR.width-4, badgeR.height), new Color(RANK_COLS[curRank].r,RANK_COLS[curRank].g,RANK_COLS[curRank].b,0.12f));
        var rankStyle = new GUIStyle(EditorStyles.boldLabel){ fontSize=18, normal={ textColor=RANK_COLS[curRank] }, alignment=TextAnchor.MiddleLeft };
        GUI.Label(new Rect(badgeR.x+12, badgeR.y, badgeR.width-12, badgeR.height), RANK_NAMES[curRank], rankStyle);

        GUILayout.Space(8);

        // Badge rang projeté
        GUILayout.Label(T("score.rankAfter"), new GUIStyle(EditorStyles.miniLabel)
            { normal={ textColor=COL_TEXT_DIM }, fontStyle=FontStyle.Bold, fontSize=9 });
        GUILayout.Space(2);
        var badgeP = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(new Rect(badgeP.x, badgeP.y, 3, badgeP.height), RANK_COLS[projRank]);
        EditorGUI.DrawRect(new Rect(badgeP.x+3, badgeP.y, badgeP.width-3, badgeP.height), new Color(RANK_COLS[projRank].r,RANK_COLS[projRank].g,RANK_COLS[projRank].b,0.08f));
        var projStyle = new GUIStyle(EditorStyles.boldLabel){ fontSize=14, normal={ textColor=RANK_COLS[projRank] }, alignment=TextAnchor.MiddleLeft };
        GUI.Label(new Rect(badgeP.x+10, badgeP.y, badgeP.width-10, badgeP.height), RANK_NAMES[projRank], projStyle);

        if (r.projectedRank < r.globalRank)
        {
            GUILayout.Space(6);
            GUILayout.Label(string.Format(T("score.levels"), r.globalRank - r.projectedRank),
                new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=COL_SUCCESS }, fontStyle=FontStyle.Bold });
        }
        EditorGUILayout.EndVertical();
        if (narrowScore) EditorGUILayout.EndVertical(); else EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(12);
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0,1,GUILayout.ExpandWidth(true)), COL_SEP);
        EditorGUILayout.Space(8);

        // ── ROW 2 : plateformes ───────────────────────────────
        EditorGUILayout.BeginHorizontal();
        DrawPlatformPanel("Quest", questPct, r.questRank, r.questProjectedRank);
        GUILayout.Space(8);
        DrawPlatformPanel("PC", pcPct, r.pcRank, r.pcProjectedRank);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        DrawLegend();
        EndCard();

        EditorGUILayout.Space(6);
        SectionLabel(T("stats.title"));

        StatRowFix(T("stat.polygons"),    r.totalPolygons,   70000, 32000, T("act.optimize"), () => _tab = 4);
        StatRowFix(T("stat.materials"),   r.totalMaterials,  32,    16,    T("act.dedup"),    () => { RunDedupMaterials(); RunAnalysis(); });
        StatRowFix(T("stat.bones"),       r.totalBones,      400,   150,   T("act.manage"),   () => _tab = 2);
        StatRowFix(T("stat.physbones"),   r.totalPhysBones,  64,    32,    T("act.manage"),   () => _tab = 1);
        StatRowFix(T("stat.blendshapes"), r.totalBlendShapes,400,   100,   T("act.clean"),    () => _tab = 3);
        StatRowFix(T("stat.lights"),      r.lightCount,      1,     1,     T("act.remove"),   () => { RemoveLights(); RunAnalysis(); });
        StatRowFix(T("stat.particles"),   r.particleCount,   10,    5,     T("act.remove"),   () => { RemoveParticles(); RunAnalysis(); });
        StatRowFix(T("stat.audio"),       r.audioSourceCount,5,     2,     T("act.remove"),   () => { RemoveAudioSources(); RunAnalysis(); });
        StatRowFix(T("stat.cameras"),     r.cameraCount,     1,     1,     T("act.remove"),   () => { RemoveCameras(); RunAnalysis(); });
        StatRowFix(T("stat.disabled"),    r.disabledObjects, 999,   999,   T("act.remove"),   () => { RemoveDisabled(); RunAnalysis(); });
        StatRowFix(T("stat.missing"),     r.missingScripts,  1,     1,     T("act.clean"),    () => { RunMissingScripts(); RunAnalysis(); });
        StatRowFix(T("stat.empty"),       r.emptyObjects,    999,   999,   T("act.clean"),    () => { RunEmptyObjects(); RunAnalysis(); });

        EditorGUILayout.Space(8);
        bool sideBySide = position.width >= 620;
        if (sideBySide) EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

        // ── QUEST ──────────────────────────────────────────────
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        SectionLabel(T("platform.quest"));
        BeginCard();
        LimitRow(T("stat.polygons"),   r.totalPolygons,    7500,  10000, 15000, 20000);
        LimitRow(T("stat.materials"),  r.totalMaterials,   1,     2,     4,     8);
        LimitRow(T("stat.physbones"),  r.totalPhysBones,   0,     4,     8,     16);
        LimitRow(T("stat.bones"),      r.totalBones,       75,    90,    150,   150);
        LimitRow(T("stat.blendshapes"),r.totalBlendShapes, 0,     7,     25,    52);
        LimitRow(T("stat.particles"),  r.particleCount,    0,     0,     0,     2);
        LimitRow(T("stat.audio"),      r.audioSourceCount, 0,     0,     1,     2);
        EndCard();
        EditorGUILayout.EndVertical();

        if (sideBySide) GUILayout.Space(8);

        // ── PC ─────────────────────────────────────────────────
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        SectionLabel(T("platform.pc"));
        BeginCard();
        LimitRow(T("stat.polygons"),   r.totalPolygons,    32000, 70000, 70000, 70000);
        LimitRow(T("stat.materials"),  r.totalMaterials,   4,     8,     16,    32);
        LimitRow(T("stat.physbones"),  r.totalPhysBones,   4,     8,     16,    32);
        LimitRow(T("stat.bones"),      r.totalBones,       75,    150,   256,   400);
        LimitRow(T("stat.blendshapes"),r.totalBlendShapes, 32,    64,    128,   256);
        LimitRow(T("stat.lights"),     r.lightCount,       0,     0,     1,     2);
        LimitRow(T("stat.particles"),  r.particleCount,    0,     4,     8,     16);
        EndCard();
        EditorGUILayout.EndVertical();

        if (sideBySide) EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        if (AccentBtn(new GUIContent(T("btn.export")), GUILayout.Height(30))) ExportReportTxt();
        EditorGUILayout.EndHorizontal();

        // ── QUEST DUPLICATE ───────────────────────────────────
        EditorGUILayout.Space(10);
        SectionLabel(T("quest.section"));
        BeginCard();
        InfoBox(T("quest.info"));
        EditorGUILayout.Space(4);

        // Dossier de sortie
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(T("lbl.folder"), new GUIStyle(EditorStyles.label){ normal={textColor=COL_TEXT_DIM}, fixedWidth=60 });
        _questOutputPath = EditorGUILayout.TextField(_questOutputPath);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);

        // Options
        EditorGUILayout.BeginHorizontal();
        _questFixShaders      = GUILayout.Toggle(_questFixShaders,      T("quest.opt.shaders"),   GUILayout.Width(140));
        _questRemoveLights    = GUILayout.Toggle(_questRemoveLights,    T("quest.opt.lights"),    GUILayout.Width(80));
        _questRemoveParticles = GUILayout.Toggle(_questRemoveParticles, T("quest.opt.particles"), GUILayout.Width(90));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        _questRemoveAudio     = GUILayout.Toggle(_questRemoveAudio,     T("quest.opt.audio"),     GUILayout.Width(140));
        _questRemoveCameras   = GUILayout.Toggle(_questRemoveCameras,   T("quest.opt.cameras"),   GUILayout.Width(80));
        _questRemoveMissing   = GUILayout.Toggle(_questRemoveMissing,   T("quest.opt.missing"),   GUILayout.Width(140));
        EditorGUILayout.EndHorizontal();

        if (_questFixShaders)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(T("quest.shader.label"), new GUIStyle(EditorStyles.label){ normal={textColor=COL_TEXT_DIM}, fixedWidth=90 });
            _questTargetShader = EditorGUILayout.Popup(_questTargetShader, _targetShaderNames);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space(6);

        var btnQuestStyle = new GUIStyle(_styleSuccessBtn){ fontSize = 13 };
        if (GUILayout.Button(T("quest.btn"), btnQuestStyle, GUILayout.Height(38)))
            RunQuestDuplicate();

        EndCard();
    }

    private void DrawCircularGauge(Rect rect, float pct)
    {
        pct = Mathf.Clamp01(pct);
        var center = new Vector2(rect.x + rect.width / 2f, rect.y + rect.height / 2f);
        float radius = Mathf.Min(rect.width, rect.height) / 2f - 10;

        int segs = 80;
        Color fillCol = pct > 0.8f ? COL_SUCCESS : pct > 0.5f ? COL_WARN : COL_ERROR;

        for (int i = 0; i < segs; i++)
        {
            float a0 = (i / (float)segs) * Mathf.PI * 2f - Mathf.PI / 2f;
            float a1 = ((i + 1) / (float)segs) * Mathf.PI * 2f - Mathf.PI / 2f;
            Vector2 p0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius;
            Vector2 p1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
            DrawLineAA(p0, p1, COL_BG3, 8);
        }

        int filledSegs = Mathf.RoundToInt(segs * pct);
        for (int i = 0; i < filledSegs; i++)
        {
            float a0 = (i / (float)segs) * Mathf.PI * 2f - Mathf.PI / 2f;
            float a1 = ((i + 1) / (float)segs) * Mathf.PI * 2f - Mathf.PI / 2f;
            Vector2 p0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius;
            Vector2 p1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
            DrawLineAA(p0, p1, fillCol, 8);
        }

        var pctText = Mathf.RoundToInt(pct * 100);
        GUI.Label(new Rect(center.x - 40, center.y - 16, 80, 32),
            pctText + "%",
            new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = fillCol }
            });
        GUI.Label(new Rect(center.x - 60, center.y + 10, 120, 20),
            "Score",
            new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = COL_TEXT_DIM }
            });
    }

    private void DrawLineAA(Vector2 a, Vector2 b, Color col, float thickness)
    {
        Handles.BeginGUI();
        var prev = Handles.color;
        Handles.color = col;
        Handles.DrawAAPolyLine(thickness, new Vector3[] { a, b });
        Handles.color = prev;
        Handles.EndGUI();
    }

    private void StatRowFix(string label, int value, int limitBad, int limitWarn, string actionLabel, System.Action onFix)
    {
        Color col = value > limitBad ? COL_ERROR : value > limitWarn ? COL_WARN : COL_SUCCESS;
        bool canFix = value > 0;
        // Reserve: dot(16) + label + value + btn + card padding(24) + scrollbar(15) ≤ position.width
        float avail  = position.width - 16 - 24 - 15;
        float btnW   = Mathf.Clamp(avail * 0.22f, 60f, 90f);
        float valueW = Mathf.Clamp(avail * 0.18f, 44f, 80f);
        float labelW = avail - btnW - valueW;

        BeginCard();
        EditorGUILayout.BeginHorizontal();
        var dotR = GUILayoutUtility.GetRect(12, 12, GUILayout.Width(12), GUILayout.Height(20));
        DrawStatusDot(new Rect(dotR.x, dotR.y + 6, 8, 8), col);
        GUILayout.Space(4);
        GUILayout.Label(label, new GUIStyle(EditorStyles.label){ normal = { textColor = COL_TEXT } }, GUILayout.Width(labelW));
        GUILayout.Label(value.ToString(), new GUIStyle(EditorStyles.boldLabel)
        { fontSize = 13, normal = { textColor = col } }, GUILayout.Width(valueW));
        GUILayout.FlexibleSpace();
        if (canFix)
        {
            Color btnBg = col == COL_SUCCESS ? COL_BG3 : col == COL_WARN ? new Color(0.6f,0.4f,0.05f) : new Color(0.6f,0.15f,0.15f);
            var btnStyle = new GUIStyle(EditorStyles.miniButton)
            {
                normal    = { background = MakeTex(1,1,btnBg), textColor = Color.white },
                hover     = { background = MakeTex(1,1,COL_ACCENT2), textColor = Color.white },
                fontSize = 10, fontStyle = FontStyle.Bold,
                fixedHeight = 20
            };
            if (GUILayout.Button(actionLabel, btnStyle, GUILayout.Width(btnW))) onFix?.Invoke();
        }
        else
        {
            GUILayout.Label("OK", new GUIStyle(EditorStyles.miniLabel)
            { normal = { textColor = COL_SUCCESS }, alignment = TextAnchor.MiddleCenter }, GUILayout.Width(btnW));
        }
        EditorGUILayout.EndHorizontal();
        EndCard();
    }

    private void LimitRow(string label, int val, int t_exc, int t_good, int t_med, int t_poor)
    {
        // Tier detection
        string tierName; Color tierCol;
        if      (val <= t_exc)  { tierName = "Excellent"; tierCol = COL_SUCCESS; }
        else if (val <= t_good) { tierName = "Good";      tierCol = new Color(0.40f,0.85f,0.20f); }
        else if (val <= t_med)  { tierName = "Medium";    tierCol = COL_WARN; }
        else if (val <= t_poor) { tierName = "Poor";      tierCol = new Color(1f,0.50f,0f); }
        else                    { tierName = "Very Poor"; tierCol = COL_ERROR; }

        int maxBar = Mathf.Max(t_poor <= 0 ? 1 : t_poor * 2, val + 1);

        // Scale fixed widths to available space (handle both full-width and half-panel contexts)
        float rowAvail  = position.width - 15; // minus scrollbar
        bool  halfPanel = position.width >= 620; // mirrors sideBySide threshold in DrawAnalyse
        float colW = halfPanel ? rowAvail * 0.48f : rowAvail;
        float lrLabelW = Mathf.Clamp(colW * 0.28f, 48f, 80f);
        float lrBadgeW = Mathf.Clamp(colW * 0.22f, 44f, 64f);
        float lrNumW   = Mathf.Clamp(colW * 0.13f, 24f, 36f);

        EditorGUILayout.BeginHorizontal(GUILayout.Height(20));

        // Label
        GUILayout.Label(label, new GUIStyle(EditorStyles.miniLabel)
            { normal={ textColor=COL_TEXT }, fontStyle=FontStyle.Bold }, GUILayout.Width(lrLabelW));

        // Tier badge
        var badge = GUILayoutUtility.GetRect(lrBadgeW, 16, GUILayout.Width(lrBadgeW), GUILayout.Height(16));
        EditorGUI.DrawRect(badge, new Color(tierCol.r,tierCol.g,tierCol.b,0.18f));
        EditorGUI.DrawRect(new Rect(badge.x, badge.y, 2, badge.height), tierCol);
        GUI.Label(badge, tierName, new GUIStyle(EditorStyles.miniLabel)
            { normal={ textColor=tierCol }, fontStyle=FontStyle.Bold, alignment=TextAnchor.MiddleCenter });

        GUILayout.Space(4);

        // Segmented bar
        var bar = GUILayoutUtility.GetRect(0, 12, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(bar, COL_BG2);

        // Tier segment backgrounds (low opacity)
        Color[] segCols = { COL_SUCCESS, new Color(0.4f,0.85f,0.2f), COL_WARN, new Color(1f,0.5f,0f), COL_ERROR };
        int[] thresholds = { t_exc, t_good, t_med, t_poor, maxBar };
        int prev = 0;
        for (int si = 0; si < 5; si++)
        {
            int thr = Mathf.Max(thresholds[si], prev);
            float x0 = (float)prev / maxBar;
            float x1 = Mathf.Clamp01((float)thr / maxBar);
            if (x1 > x0) EditorGUI.DrawRect(new Rect(bar.x + bar.width*x0, bar.y, bar.width*(x1-x0), bar.height),
                new Color(segCols[si].r, segCols[si].g, segCols[si].b, 0.10f));
            prev = thr;
        }

        // Fill (current value)
        float fill = Mathf.Clamp01((float)val / maxBar);
        EditorGUI.DrawRect(new Rect(bar.x, bar.y, bar.width*fill, bar.height),
            new Color(tierCol.r, tierCol.g, tierCol.b, 0.75f));

        // Threshold tick marks (couleur du tier suivant, semi-transparent)
        Color[] tickCols = { new Color(0.4f,0.85f,0.2f), COL_WARN, new Color(1f,0.5f,0f), COL_ERROR };
        int[] tickThrs   = { t_exc, t_good, t_med, t_poor };
        for (int ti = 0; ti < 4; ti++)
        {
            float tx = (float)tickThrs[ti] / maxBar;
            if (tx > 0f && tx < 1f)
                EditorGUI.DrawRect(new Rect(bar.x + bar.width*tx - 1, bar.y, 2, bar.height),
                    new Color(tickCols[ti].r, tickCols[ti].g, tickCols[ti].b, 0.55f));
        }

        GUILayout.Space(4);

        // Value + poor limit
        string valStr = val >= 1000 ? (val/1000) + "k" : val.ToString();
        string limStr = t_poor >= 1000 ? (t_poor/1000) + "k" : t_poor.ToString();
        GUILayout.Label(valStr, new GUIStyle(EditorStyles.boldLabel)
            { normal={ textColor=tierCol }, fontSize=10 }, GUILayout.Width(lrNumW));
        GUILayout.Label("/" + limStr, new GUIStyle(EditorStyles.miniLabel)
            { normal={ textColor=COL_TEXT_DIM } }, GUILayout.Width(lrNumW));

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
    }

    private void DrawPlatformPanel(string title, float pct, int rank, int projectedRank)
    {
        int ri = Mathf.Clamp(rank,          0, 4);
        int pi = Mathf.Clamp(projectedRank, 0, 4);
        Color rankCol = pct >= 0.8f ? COL_SUCCESS : pct >= 0.5f ? COL_WARN : COL_ERROR;

        EditorGUILayout.BeginVertical(new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTex(1,1,COL_BG3) },
            padding = new RectOffset(12,12,10,10),
            border  = new RectOffset(1,1,1,1)
        }, GUILayout.ExpandWidth(true));

        // Header
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(title, new GUIStyle(EditorStyles.boldLabel){ normal={ textColor=COL_TEXT }, fontSize=13 });
        GUILayout.FlexibleSpace();
        GUILayout.Label(Mathf.RoundToInt(pct * 100) + "%",
            new GUIStyle(EditorStyles.boldLabel){ normal={ textColor=rankCol }, fontSize=16, alignment=TextAnchor.MiddleRight });
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(4);

        // Progress bar
        var barRect = GUILayoutUtility.GetRect(0, 8, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(barRect, COL_BG2);
        EditorGUI.DrawRect(new Rect(barRect.x, barRect.y, barRect.width * pct, barRect.height), rankCol);
        // Round end cap
        if (pct > 0.01f && pct < 0.99f)
            EditorGUI.DrawRect(new Rect(barRect.x + barRect.width*pct - 2, barRect.y, 4, barRect.height), rankCol);

        GUILayout.Space(6);

        // Ranks
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(T("lbl.current"), new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=COL_TEXT_DIM } }, GUILayout.Width(50));
        GUILayout.Label(RANK_NAMES[ri], new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=RANK_COLS[ri] }, fontStyle=FontStyle.Bold });
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(T("lbl.after"), new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=COL_TEXT_DIM } }, GUILayout.Width(50));
        GUILayout.Label(RANK_NAMES[pi], new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=RANK_COLS[pi] }, fontStyle=FontStyle.Bold });
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawPlatformScorePanel(string title, int rank, int projectedRank)
    {
        EditorGUILayout.BeginVertical(_styleCard, GUILayout.Width(130));
        GUILayout.Label(title, new GUIStyle(EditorStyles.boldLabel)
        { fontSize = 12, normal = { textColor = COL_TEXT } });
        GUILayout.Space(4);
        GUILayout.Label(T("lbl.current") + ": " + RANK_NAMES[Mathf.Clamp(rank,0,4)], new GUIStyle(EditorStyles.label)
        { normal = { textColor = COL_TEXT } });
        GUILayout.Label(T("lbl.after") + ": " + RANK_NAMES[Mathf.Clamp(projectedRank,0,4)], new GUIStyle(EditorStyles.boldLabel)
        { normal = { textColor = RANK_COLS[Mathf.Clamp(projectedRank,0,4)] } });
        EditorGUILayout.EndVertical();
    }

    private void DrawPlatformCircularGauge(string title, float pct, int rank, int projectedRank)
    {
        EditorGUILayout.BeginVertical(_styleCard, GUILayout.Width(130));
        GUILayout.Label(title, new GUIStyle(EditorStyles.boldLabel)
        { fontSize = 12, normal = { textColor = COL_TEXT } });
        GUILayout.Space(4);
        var gaugeRect = GUILayoutUtility.GetRect(100, 100, GUILayout.Width(100), GUILayout.Height(100));
        DrawSmallCircularGauge(gaugeRect, pct);
        GUILayout.Space(6);
        GUILayout.Label("Actuel : " + RANK_NAMES[Mathf.Clamp(rank,0,4)], new GUIStyle(EditorStyles.miniLabel)
        { normal = { textColor = COL_TEXT_DIM } });
        GUILayout.Label("Apres : " + RANK_NAMES[Mathf.Clamp(projectedRank,0,4)], new GUIStyle(EditorStyles.miniLabel)
        { normal = { textColor = COL_TEXT_DIM } });
        EditorGUILayout.EndVertical();
    }

    private void DrawLegend()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);
        DrawLegendItem(COL_SUCCESS, T("legend.good"));
        DrawLegendItem(COL_WARN,    T("legend.medium"));
        DrawLegendItem(COL_ERROR,   T("legend.poor"));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLegendItem(Color color, string label)
    {
        var rect = GUILayoutUtility.GetRect(16, 16, GUILayout.Width(16), GUILayout.Height(16));
        EditorGUI.DrawRect(rect, color);
        GUILayout.Space(4);
        GUILayout.Label(label, new GUIStyle(EditorStyles.miniLabel)
        { normal = { textColor = COL_TEXT_DIM } });
        GUILayout.Space(16);
    }

    private void DrawSmallCircularGauge(Rect rect, float pct)
    {
        var center = new Vector2(rect.x + rect.width / 2f, rect.y + rect.height / 2f);
        float radius = Mathf.Min(rect.width, rect.height) / 2f - 8;
        int segs = 60;
        Color fillCol = pct > 0.8f ? COL_SUCCESS : pct > 0.5f ? COL_WARN : COL_ERROR;

        for (int i = 0; i < segs; i++)
        {
            float a0 = (i / (float)segs) * Mathf.PI * 2f - Mathf.PI / 2f;
            float a1 = ((i + 1) / (float)segs) * Mathf.PI * 2f - Mathf.PI / 2f;
            Vector2 p0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius;
            Vector2 p1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
            DrawLineAA(p0, p1, COL_BG3, 6);
        }

        int filledSegs = Mathf.RoundToInt(segs * pct);
        for (int i = 0; i < filledSegs; i++)
        {
            float a0 = (i / (float)segs) * Mathf.PI * 2f - Mathf.PI / 2f;
            float a1 = ((i + 1) / (float)segs) * Mathf.PI * 2f - Mathf.PI / 2f;
            Vector2 p0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius;
            Vector2 p1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
            DrawLineAA(p0, p1, fillCol, 6);
        }

        GUI.Label(new Rect(center.x - 30, center.y - 16, 60, 32), Mathf.RoundToInt(pct * 100f) + "%",
            new GUIStyle(EditorStyles.boldLabel)
            { alignment = TextAnchor.MiddleCenter, normal = { textColor = fillCol } });
    }

    private float CalcPlatformScorePct(AnalyseResults r, bool quest)
    {
        float polyGoal = quest ? 32000f : 70000f;
        float matGoal = quest ? 8f : 16f;
        float physGoal = quest ? 16f : 32f;
        float boneGoal = quest ? 75f : 150f;
        float blendGoal = quest ? 52f : 100f;

        float polyPct = r.totalPolygons <= 0 ? 1f : Mathf.Clamp01(polyGoal / r.totalPolygons);
        float matPct = r.totalMaterials <= 0 ? 1f : Mathf.Clamp01(matGoal / r.totalMaterials);
        float physPct = r.totalPhysBones <= 0 ? 1f : Mathf.Clamp01(physGoal / r.totalPhysBones);
        float bonePct = r.totalBones <= 0 ? 1f : Mathf.Clamp01(boneGoal / r.totalBones);
        float blendPct = r.totalBlendShapes <= 0 ? 1f : Mathf.Clamp01(blendGoal / r.totalBlendShapes);
        float lightPct = r.lightCount == 0 ? 1f : Mathf.Clamp01(1f - (r.lightCount / 2f));
        float particlePct = r.particleCount <= 5 ? 1f : Mathf.Clamp01(1f - ((r.particleCount - 5) / 10f));

        float total = polyPct + matPct + physPct + bonePct + blendPct + lightPct + particlePct;
        return Mathf.Clamp01(total / 7f);
    }

    private float CalcGlobalScorePct(AnalyseResults r)
    {
        float polyPct = r.totalPolygons <= 32000 ? 1f : r.totalPolygons >= 70000 ? 0f : 1f - ((r.totalPolygons - 32000f) / 38000f);
        float matPct = r.totalMaterials <= 16 ? 1f : r.totalMaterials >= 32 ? 0f : 1f - ((r.totalMaterials - 16f) / 16f);
        float physPct = r.totalPhysBones <= 32 ? 1f : r.totalPhysBones >= 64 ? 0f : 1f - ((r.totalPhysBones - 32f) / 32f);
        float bonePct = r.totalBones <= 150 ? 1f : r.totalBones >= 400 ? 0f : 1f - ((r.totalBones - 150f) / 250f);
        float blendPct = r.totalBlendShapes <= 100 ? 1f : r.totalBlendShapes >= 400 ? 0f : 1f - ((r.totalBlendShapes - 100f) / 300f);
        float lightPct = r.lightCount == 0 ? 1f : 0f;
        float particlePct = r.particleCount <= 5 ? 1f : Mathf.Clamp01(1f - ((r.particleCount - 5f) / 20f));

        float total = polyPct + matPct + physPct + bonePct + blendPct + lightPct + particlePct;
        return Mathf.Clamp01(total / 7f);
    }

    private void ExportReportTxt()
    {
        var path = EditorUtility.SaveFilePanel("Export rapport", "", _avatar.name + "_report.txt", "txt");
        if (string.IsNullOrEmpty(path)) return;
        var r = _results;
        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════");
        sb.AppendLine("NETRA Avatar Optimizer — Rapport");
        sb.AppendLine("Avatar : " + _avatar.name);
        sb.AppendLine("Date : " + DateTime.Now);
        sb.AppendLine("═══════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine("RANG ACTUEL : " + RANK_NAMES[Mathf.Clamp(r.globalRank, 0, 4)]);
        sb.AppendLine("RANG QUEST : " + RANK_NAMES[Mathf.Clamp(r.questRank, 0, 4)]);
        sb.AppendLine("RANG PC : " + RANK_NAMES[Mathf.Clamp(r.pcRank, 0, 4)]);
        sb.AppendLine("RANG APRES OPTIMISATION : " + RANK_NAMES[Mathf.Clamp(r.projectedRank, 0, 4)]);
        sb.AppendLine("RANG APRES OPTIMISATION (QUEST) : " + RANK_NAMES[Mathf.Clamp(r.questProjectedRank, 0, 4)]);
        sb.AppendLine("RANG APRES OPTIMISATION (PC) : " + RANK_NAMES[Mathf.Clamp(r.pcProjectedRank, 0, 4)]);
        sb.AppendLine();
        sb.AppendLine("─── STATISTIQUES ───");
        sb.AppendLine("Objets total       : " + r.totalObjects);
        sb.AppendLine("Polygones          : " + r.totalPolygons);
        sb.AppendLine("Materiaux          : " + r.totalMaterials);
        sb.AppendLine("Os                 : " + r.totalBones);
        sb.AppendLine("PhysBones          : " + r.totalPhysBones);
        sb.AppendLine("PhysBone Colliders : " + r.totalPhysBoneColliders);
        sb.AppendLine("BlendShapes        : " + r.totalBlendShapes);
        sb.AppendLine("Lights             : " + r.lightCount);
        sb.AppendLine("Particles          : " + r.particleCount);
        sb.AppendLine("AudioSources       : " + r.audioSourceCount);
        sb.AppendLine("Cameras            : " + r.cameraCount);
        sb.AppendLine("Objets desactives  : " + r.disabledObjects);
        sb.AppendLine("Scripts manquants  : " + r.missingScripts);
        sb.AppendLine("Objets vides       : " + r.emptyObjects);
        try
        {
            File.WriteAllText(path, sb.ToString());
            Log(LogLevel.Success, "Rapport exporte vers " + path);
            EditorUtility.DisplayDialog("Export", "Rapport sauvegarde :\n" + path, "OK");
        }
        catch (Exception e)
        {
            Log(LogLevel.Error, "Erreur lors de l'export du rapport : " + e.Message);
            EditorUtility.DisplayDialog("Erreur", "Impossible d'exporter le rapport :\n" + e.Message, "OK");
        }
    }

    // ═════════════════════════════════════════════════════════════
    //  FIX ACTIONS (utilises par StatRowFix)
    // ═════════════════════════════════════════════════════════════
    private void RemoveLights()
    {
        if (_avatar == null) return;
        var t = GetTarget();
        Undo.RegisterFullObjectHierarchyUndo(t, "Remove Lights");
        var arr = t.GetComponentsInChildren<Light>(true);
        foreach (var l in arr) Undo.DestroyObjectImmediate(l);
        SaveWorkingCopy();
        Log(LogLevel.Success, arr.Length + " light(s) supprimee(s)");
    }
    private void RemoveParticles()
    {
        if (_avatar == null) return;
        var t = GetTarget();
        Undo.RegisterFullObjectHierarchyUndo(t, "Remove Particles");
        var arr = t.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var p in arr) Undo.DestroyObjectImmediate(p);
        SaveWorkingCopy();
        Log(LogLevel.Success, arr.Length + " particle system(s) supprime(s)");
    }
    private void RemoveAudioSources()
    {
        if (_avatar == null) return;
        var t = GetTarget();
        Undo.RegisterFullObjectHierarchyUndo(t, "Remove AudioSources");
        var arr = t.GetComponentsInChildren<AudioSource>(true);
        foreach (var a in arr) Undo.DestroyObjectImmediate(a);
        SaveWorkingCopy();
        Log(LogLevel.Success, arr.Length + " AudioSource(s) supprime(s)");
    }
    private void RemoveCameras()
    {
        if (_avatar == null) return;
        var t = GetTarget();
        Undo.RegisterFullObjectHierarchyUndo(t, "Remove Cameras");
        var arr = t.GetComponentsInChildren<Camera>(true);
        foreach (var c in arr) Undo.DestroyObjectImmediate(c);
        SaveWorkingCopy();
        Log(LogLevel.Success, arr.Length + " Camera(s) supprimee(s)");
    }
    private void RemoveDisabled()
    {
        if (_avatar == null) return;
        var t = GetTarget();
        Undo.RegisterFullObjectHierarchyUndo(t, "Remove Disabled");
        var list = t.GetComponentsInChildren<Transform>(true)
            .Where(x => x != t.transform && !x.gameObject.activeSelf)
            .Select(x => x.gameObject).ToList();
        foreach (var go in list) Undo.DestroyObjectImmediate(go);
        SaveWorkingCopy();
        Log(LogLevel.Success, list.Count + " objet(s) desactive(s) supprime(s)");
    }

    private void RunFullOptimization()
    {
        if (_avatar == null) return;
        var t = GetTarget();
        if (_opt_backup) DoBackup();
        Undo.RegisterFullObjectHierarchyUndo(t, "Full Optimization");

        int total = 0;
        total += RunMissingScripts();
        total += RunEmptyObjects();
        total += RunDedupMaterials();
        int l = t.GetComponentsInChildren<Light>(true).Length;
        foreach (var x in t.GetComponentsInChildren<Light>(true)) Undo.DestroyObjectImmediate(x);
        int p = t.GetComponentsInChildren<ParticleSystem>(true).Length;
        foreach (var x in t.GetComponentsInChildren<ParticleSystem>(true)) Undo.DestroyObjectImmediate(x);
        int a = t.GetComponentsInChildren<AudioSource>(true).Length;
        foreach (var x in t.GetComponentsInChildren<AudioSource>(true)) Undo.DestroyObjectImmediate(x);
        int c = t.GetComponentsInChildren<Camera>(true).Length;
        foreach (var x in t.GetComponentsInChildren<Camera>(true)) Undo.DestroyObjectImmediate(x);
        total += l + p + a + c;
        SaveWorkingCopy();
        Log(LogLevel.Success, "Optimisation complete : " + total + " modification(s)");
        RunAnalysis();
        EditorUtility.DisplayDialog("Termine", "Optimisation complete appliquee.\n" + total + " modification(s).\n" + (_dupMode ? "Sauvegardé : " + _workingPrefabPath : ""), "OK");
    }

    // ─── QUEST DUPLICATE ─────────────────────────────────────────
    private void RunQuestDuplicate()
    {
        if (_avatar == null) { EditorUtility.DisplayDialog("Erreur","Assigne un avatar !","OK"); return; }

        string avatarName = _avatar.name;
        _questOutputPath = NormalizeAssetPath(_questOutputPath);
        if (string.IsNullOrEmpty(_questOutputPath) || !_questOutputPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            EditorUtility.DisplayDialog("Erreur", "Le dossier de sortie Quest doit être un chemin relatif commençant par Assets/.", "OK");
            return;
        }

        string outRoot   = _questOutputPath;
        string outFolder = outRoot + "/" + avatarName;
        string matFolder = outFolder + "/Materials";

        // ── 1. Créer les dossiers ─────────────────────────────
        EnsureFolder(outRoot);
        EnsureFolder(outFolder);
        EnsureFolder(matFolder);

        // ── 2. Instancier la copie ────────────────────────────
        GameObject questGO = Instantiate(_avatar);
        questGO.name = avatarName + "_Quest";
        // Décaler légèrement pour ne pas superposer l'original
        questGO.transform.position = _avatar.transform.position + new Vector3(2f, 0f, 0f);

        // ── 3. Dupliquer les matériaux avec shader Quest ──────
        var questShader = _questFixShaders ? Shader.Find(_targetShaderNames[_questTargetShader]) : null;
        if (_questFixShaders && questShader == null)
        {
            EditorUtility.DisplayDialog("Erreur", "Shader Quest introuvable : " + _targetShaderNames[_questTargetShader], "OK");
            DestroyImmediate(questGO);
            return;
        }
        var matMap = new Dictionary<Material, Material>();

        foreach (var rend in questGO.GetComponentsInChildren<Renderer>(true))
        {
            var mats = rend.sharedMaterials;
            bool dirty = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                if (!matMap.TryGetValue(mats[i], out var qMat))
                {
                    qMat = new Material(mats[i]);
                    qMat.name = mats[i].name + "_Quest";
                    if (questShader != null) qMat.shader = questShader;
                    string mp = AssetDatabase.GenerateUniqueAssetPath(matFolder + "/" + qMat.name + ".mat");
                    AssetDatabase.CreateAsset(qMat, mp);
                    matMap[mats[i]] = qMat;
                }
                mats[i] = qMat;
                dirty = true;
            }
            if (dirty) rend.sharedMaterials = mats;
        }

        // ── 4. Supprimer les composants Quest-incompatibles ───
        if (_questRemoveLights)
            foreach (var x in questGO.GetComponentsInChildren<Light>(true)) DestroyImmediate(x);
        if (_questRemoveParticles)
            foreach (var x in questGO.GetComponentsInChildren<ParticleSystem>(true)) DestroyImmediate(x);
        if (_questRemoveAudio)
            foreach (var x in questGO.GetComponentsInChildren<AudioSource>(true)) DestroyImmediate(x);
        if (_questRemoveCameras)
            foreach (var x in questGO.GetComponentsInChildren<Camera>(true)) DestroyImmediate(x);
        if (_questRemoveMissing)
            foreach (var go in questGO.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject))
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

        // ── 5. Optimiser PhysBones / Colliders (preset Quest) ─
        var questPhys = new List<PhysEntry>();
        foreach (var c in questGO.GetComponentsInChildren<Component>(true))
        {
            if (c == null) continue;
            string ct = c.GetType().Name;
            if (ct != "VRCPhysBone" && ct != "VRCPhysBoneCollider") continue;
            questPhys.Add(new PhysEntry {
                component  = c,
                boneName   = c.gameObject.name,
                isCollider = ct == "VRCPhysBoneCollider",
                childCount = c.transform.GetComponentsInChildren<Transform>(true).Length,
                keep       = true
            });
        }
        // Appliquer le preset Quest (ears first) sur la copie
        var qPbs  = questPhys.Where(b => !b.isCollider)
            .OrderByDescending(b => GetQuestPriority(b.boneName))
            .ThenBy(b => b.childCount).ToList();
        var qCols = questPhys.Where(b => b.isCollider)
            .OrderByDescending(b => GetQuestPriority(b.boneName))
            .ToList();
        int qSelPhys = 0, qSelTrsf = 0;
        foreach (var pb in qPbs)
        {
            if (qSelPhys >= LIM_PB || qSelTrsf + pb.childCount > LIM_TRSF) { pb.keep = false; continue; }
            qSelPhys++; qSelTrsf += pb.childCount;
        }
        int qMaxCols = qSelPhys == 0 ? LIM_COL : Mathf.Min(LIM_COL, LIM_CC / (2 * qSelPhys));
        for (int i = 0; i < qCols.Count; i++) qCols[i].keep = i < qMaxCols;
        foreach (var b in questPhys.Where(b => !b.keep && b.component != null))
            DestroyImmediate(b.component);
        Log(LogLevel.Success, "Quest physics : " + qSelPhys + " PB, " + Mathf.Min(qCols.Count, qMaxCols) + " Col gardes");

        // ── 6. Dupliquer les FBX + compression mesh Medium ───
        string fbxFolder = outFolder + "/FBX";
        EnsureFolder(fbxFolder);
        var fbxMap = DuplicateFBXFiles(questGO, fbxFolder);
        if (fbxMap.Count > 0) RemapMeshesToFBX(questGO, fbxMap);
        ApplyMeshCompressionOnTarget(questGO, ModelImporterMeshCompression.Medium, 2);

        // ── 7. Dupliquer + optimiser les textures Quest ──────────────
        string texFolder = outFolder + "/Textures";
        EnsureFolder(texFolder);
        var texMap = DuplicateAndOptimizeTextures(questGO, texFolder);
        if (texMap.Count > 0) RemapMaterialTextures(matMap.Values, texMap);

        // ── 8. Sauvegarder comme prefab ───────────────────────
        string prefabPath = AssetDatabase.GenerateUniqueAssetPath(outFolder + "/" + questGO.name + ".prefab");
        PrefabUtility.SaveAsPrefabAssetAndConnect(questGO, prefabPath, InteractionMode.UserAction);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int mCount = matMap.Count;
        Log(LogLevel.Success, "Version Quest créée → " + prefabPath + " | " + mCount + " mat | " + qSelPhys + " PB | " + fbxMap.Count + " FBX");
        EditorUtility.DisplayDialog("Quest créé ✓",
            "Avatar Quest : " + questGO.name +
            "\nPrefab       : " + prefabPath +
            "\nMatériaux    : " + mCount + " copie(s)" +
            "\nFBX dupliques : " + fbxMap.Count + " fichier(s) → " + fbxFolder +
            "\nPhysBones    : " + qSelPhys + " / max " + LIM_PB +
            "\nMesh         : compression Medium + blend shapes viseme" +
            "\n\nL'original est intact.",
            "OK");
    }

    private Dictionary<Texture, Texture> DuplicateAndOptimizeTextures(GameObject target, string texOutDir)
    {
        // Collect all textures referenced by the target's renderers
        var texPaths = new Dictionary<Texture, string>();
        foreach (var r in target.GetComponentsInChildren<Renderer>(true))
        {
            if (r == null) continue;
            foreach (var mat in r.sharedMaterials)
            {
                if (mat == null) continue;
                int cnt = ShaderUtil.GetPropertyCount(mat.shader);
                for (int i = 0; i < cnt; i++)
                {
                    if (ShaderUtil.GetPropertyType(mat.shader, i) != ShaderUtil.ShaderPropertyType.TexEnv) continue;
                    var t = mat.GetTexture(ShaderUtil.GetPropertyName(mat.shader, i));
                    if (t != null && !texPaths.ContainsKey(t))
                    {
                        string p = AssetDatabase.GetAssetPath(t);
                        if (!string.IsNullOrEmpty(p)) texPaths[t] = p;
                    }
                }
            }
        }

        // Copy each texture file to the quest folder
        var srcToDest = new Dictionary<string, string>();
        foreach (var kv in texPaths)
        {
            string fname = Path.GetFileName(kv.Value);
            string dest  = AssetDatabase.GenerateUniqueAssetPath(texOutDir + "/" + fname);
            if (AssetDatabase.CopyAsset(kv.Value, dest))
                srcToDest[kv.Value] = dest;
            else
                Log(LogLevel.Warn, "Texture copy echoue : " + kv.Value);
        }

        if (srcToDest.Count > 0) { AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }

        // Apply Android import settings on copies only
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var kv in srcToDest)
            {
                var imp = AssetImporter.GetAtPath(kv.Value) as TextureImporter;
                if (imp == null) continue;
                var origTex = texPaths.FirstOrDefault(t => t.Value == kv.Key).Key;
                int targetSize = origTex != null && (origTex.width > 1024 || origTex.height > 1024) ? 1024 : 512;
                var ps = new TextureImporterPlatformSettings
                {
                    name               = "Android",
                    overridden         = true,
                    maxTextureSize     = targetSize,
                    format             = TextureImporterFormat.ASTC_6x6,
                    textureCompression = TextureImporterCompression.Compressed
                };
                imp.SetPlatformTextureSettings(ps);
                imp.mipmapEnabled    = true;
                imp.streamingMipmaps = true;
                imp.SaveAndReimport();
            }
        }
        finally { AssetDatabase.StopAssetEditing(); }

        AssetDatabase.Refresh();

        // Build final map: original Texture → copied Texture
        var result = new Dictionary<Texture, Texture>();
        foreach (var kv in texPaths)
        {
            if (!srcToDest.TryGetValue(kv.Value, out string dest)) continue;
            var newTex = AssetDatabase.LoadAssetAtPath<Texture>(dest);
            if (newTex != null) result[kv.Key] = newTex;
        }

        Log(LogLevel.Success, "Quest textures : " + result.Count + " texture(s) dupliquee(s) et optimisees (ASTC 6x6, max 1024px)");
        return result;
    }

    private void RemapMaterialTextures(IEnumerable<Material> materials, Dictionary<Texture, Texture> texMap)
    {
        foreach (var mat in materials)
        {
            if (mat == null) continue;
            bool dirty = false;
            int cnt = ShaderUtil.GetPropertyCount(mat.shader);
            for (int i = 0; i < cnt; i++)
            {
                if (ShaderUtil.GetPropertyType(mat.shader, i) != ShaderUtil.ShaderPropertyType.TexEnv) continue;
                string propName = ShaderUtil.GetPropertyName(mat.shader, i);
                var t = mat.GetTexture(propName);
                if (t != null && texMap.TryGetValue(t, out var newTex))
                {
                    mat.SetTexture(propName, newTex);
                    dirty = true;
                }
            }
            if (dirty) EditorUtility.SetDirty(mat);
        }
    }

    private Dictionary<string, string> DuplicateFBXFiles(GameObject target, string fbxOutDir)
    {
        var pathMap = new Dictionary<string, string>();
        var origPaths = new HashSet<string>();

        foreach (var smr in target.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr?.sharedMesh == null) continue;
            string p = AssetDatabase.GetAssetPath(smr.sharedMesh);
            if (string.IsNullOrEmpty(p)) continue;
            string ext = Path.GetExtension(p).ToLowerInvariant();
            if (ext == ".fbx" || ext == ".obj" || ext == ".dae" || ext == ".blend")
                origPaths.Add(p);
        }

        foreach (var src in origPaths)
        {
            // Already inside the output dir → already a duplicate, skip
            string normalSrc = NormalizeAssetPath(src);
            string normalDir = NormalizeAssetPath(fbxOutDir);
            if (normalSrc.StartsWith(normalDir + "/"))
                continue;

            string fname = Path.GetFileName(src);
            string dest  = fbxOutDir + "/" + fname;

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dest) != null)
            {
                // Duplicate already exists → reuse it without creating another copy
                pathMap[src] = dest;
                Log(LogLevel.Info, "FBX existant reutilise : " + dest);
            }
            else if (AssetDatabase.CopyAsset(src, dest))
            {
                pathMap[src] = dest;
                Log(LogLevel.Info, "FBX duplique : " + src + " → " + dest);
            }
            else
                Log(LogLevel.Warn, "FBX copy echoue : " + src);
        }

        if (pathMap.Count > 0) { AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }
        return pathMap;
    }

    private void RemapMeshesToFBX(GameObject target, Dictionary<string, string> fbxMap)
    {
        // Construit un dictionnaire nom → nouveau mesh depuis les FBX copiés
        var meshByName = new Dictionary<string, Mesh>();
        foreach (var kv in fbxMap)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(kv.Value))
            {
                if (asset is Mesh mm && !meshByName.ContainsKey(mm.name))
                    meshByName[mm.name] = mm;
            }
        }

        foreach (var smr in target.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr?.sharedMesh == null) continue;
            string origPath = AssetDatabase.GetAssetPath(smr.sharedMesh);
            if (!fbxMap.ContainsKey(origPath)) continue;
            if (meshByName.TryGetValue(smr.sharedMesh.name, out var newM))
            {
                smr.sharedMesh = newM;
                EditorUtility.SetDirty(smr);
            }
        }
    }

    private static string NormalizeAssetPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;
        path = path.Replace("\\", "/").Trim();
        while (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
        return path;
    }

    // ─── COPIE DE TRAVAIL ─────────────────────────────────────
    private GameObject GetTarget()
    {
        return _avatar;
    }

    private void EnsureWorkingCopy()
    {
        // Désactivé : aucune copie de travail ne doit être créée.
    }

    private void SaveWorkingCopy()
    {
        if (_workingCopy == null || string.IsNullOrEmpty(_workingPrefabPath)) return;
        PrefabUtility.SaveAsPrefabAssetAndConnect(_workingCopy, _workingPrefabPath, InteractionMode.UserAction);
        AssetDatabase.SaveAssets();
    }

    private static void EnsureFolder(string path)
    {
        if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        if (slash <= 0) return;
        string parent = path.Substring(0, slash);
        string child  = path.Substring(slash + 1);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, child);
    }

    // ═════════════════════════════════════════════════════════════
    //  TAB 1 — PHYSICS
    // ═════════════════════════════════════════════════════════════
    private void DrawPhysics()
    {
        if (_avatar == null) { InfoBox(T("msg.no.avatar.top")); return; }

        SectionLabel(T("physics.section"));
        InfoBox(T("physics.info"));
        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();
        if (AccentBtn(new GUIContent(T("btn.scan"),"Scanne tous les VRCPhysBone et VRCPhysBoneCollider"), GUILayout.Height(32), GUILayout.ExpandWidth(true))) ScanPhysics();
        if (SuccessBtn(new GUIContent(T("physics.preset.btn"),"Reduit automatiquement a max 8 PhysBones et 16 Colliders"), GUILayout.Height(32), GUILayout.Width(150))) ApplyQuestPreset();
        if (UndoBtn(new GUIContent(T("btn.undo"),"Ctrl+Z"), GUILayout.Height(32), GUILayout.Width(90))) { Undo.PerformUndo(); _physScanned = false; }
        EditorGUILayout.EndHorizontal();

        if (!_physScanned || _phys.Count == 0)
        {
            if (_physScanned) WarnBox(T("physics.none.found"));
            return;
        }

        EditorGUILayout.Space(8);
        var pbs  = _phys.Where(b => !b.isCollider).ToList();
        var cols = _phys.Where(b =>  b.isCollider).ToList();
        DrawPhysSection(T("physics.physbones"), pbs, ref _physScrollPB);
        EditorGUILayout.Space(6);
        DrawPhysSection(T("physics.colliders"), cols, ref _physScrollCol);

        EditorGUILayout.Space(10);
        SectionLabel(T("physics.perf"));
        BeginCard();
        DrawPerfStats();
        EndCard();

        EditorGUILayout.Space(8);
        int toDelete = _phys.Count(b => !b.keep);
        EditorGUI.BeginDisabledGroup(toDelete == 0);
        if (DangerBtn(toDelete > 0
            ? T("act.remove") + " (" + toDelete + ")"
            : T("physics.none.found"), GUILayout.Height(36)))
        {
            if (EditorUtility.DisplayDialog(T("dlg.confirm"),
                string.Format(T("physics.delete.confirm"), toDelete), T("dlg.delete"), T("dlg.cancel")))
                ApplyPhysRemovals();
        }
        EditorGUI.EndDisabledGroup();
    }

    private void ApplyQuestPreset()
    {
        var pbs  = _phys.Where(b => !b.isCollider)
            .OrderByDescending(b => GetQuestPriority(b.boneName))
            .ThenBy(b => b.childCount)
            .ToList();
        var cols = _phys.Where(b => b.isCollider)
            .OrderByDescending(b => GetQuestPriority(b.boneName))
            .ToList();

        int selectedPhys = 0;
        int selectedTransforms = 0;
        foreach (var pb in pbs)
        {
            if (selectedPhys >= LIM_PB) { pb.keep = false; continue; }
            if (pb.childCount > LIM_TRSF) { pb.keep = false; continue; }

            bool canKeep = selectedTransforms + pb.childCount <= LIM_TRSF;
            pb.keep = canKeep;
            if (canKeep)
            {
                selectedPhys++;
                selectedTransforms += pb.childCount;
            }
        }

        int maxCols = selectedPhys == 0 ? LIM_COL : Mathf.Min(LIM_COL, LIM_CC / (2 * selectedPhys));
        int selectedCols = 0;
        for (int i = 0; i < cols.Count; i++)
        {
            cols[i].keep = i < maxCols;
            if (cols[i].keep) selectedCols++;
        }

        int collisionChecks = selectedPhys * selectedCols * 2;
        Log(LogLevel.Success, "Preset Quest applique : " + selectedPhys + " PhysBones, " + selectedTransforms + " transforms, " + selectedCols + " Colliders, " + collisionChecks + " Collision Checks");
    }

    private int GetQuestPriority(string boneName)
    {
        var n = boneName.ToLowerInvariant();
        // Visage / tête
        if (n.Contains("head")   || n.Contains("tete"))                             return 10;
        if (n.Contains("ear")    || n.Contains("oreille"))                          return 10;
        // Colonne vertébrale / corps
        if (n.Contains("spine")  || n.Contains("colonne"))                          return 9;
        if (n.Contains("hip")    || n.Contains("pelvis") || n.Contains("bassin"))   return 9;
        if (n.Contains("chest")  || n.Contains("breast") || n.Contains("poitrine")) return 8;
        // Cheveux / queue / ailes
        if (n.Contains("hair")   || n.Contains("cheveu"))                           return 6;
        if (n.Contains("tail")   || n.Contains("queue"))                            return 5;
        if (n.Contains("wing")   || n.Contains("aile"))                             return 4;
        // Vêtements
        if (n.Contains("cloth")  || n.Contains("skirt") || n.Contains("jupe"))     return 2;
        return 0;
    }

    private void KeepTopNPhys(int n)
    {
        var pbs = _phys.Where(b => !b.isCollider).OrderByDescending(b => b.childCount).ToList();
        for (int i = 0; i < pbs.Count; i++) pbs[i].keep = i < n;
        Log(LogLevel.Success, "Top " + n + " PhysBones conserves");
    }

    private void DrawPhysSection(string title, List<PhysEntry> items, ref Vector2 scroll)
    {
        if (items.Count == 0) return;
        int kept = items.Count(b => b.keep);
        SectionLabel(title.ToUpper() + " — " + kept + " / " + items.Count + " " + T("physics.kept"));
        BeginCard();
        EditorGUILayout.BeginHorizontal();
        if (SmallBtn(T("btn.check.all")))   items.ForEach(b => b.keep = true);
        if (DangerSmallBtn(T("btn.uncheck.all"))) items.ForEach(b => b.keep = false);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MaxHeight(Mathf.Min(items.Count * 26 + 8, 260)));
        int _physRowIdx = 0;
        foreach (var b in items)
        {
            if (b.component == null) continue;
            var row = EditorGUILayout.BeginHorizontal(GUILayout.Height(24));
            EditorGUI.DrawRect(row, _physRowIdx++ % 2 == 0 ? new Color(0,0,0,0) : new Color(1,1,1,0.04f));
            if (!b.keep) EditorGUI.DrawRect(row, new Color(COL_ERROR.r,COL_ERROR.g,COL_ERROR.b,0.15f));

            bool nk = EditorGUILayout.Toggle(b.keep, GUILayout.Width(18));
            if (nk != b.keep) { b.keep = nk; Repaint(); }

            Color tc = b.keep ? COL_ACCENT2 : new Color(COL_ERROR.r, COL_ERROR.g, COL_ERROR.b, 0.8f);
            var ns = new GUIStyle(_styleLink){ normal = { textColor = tc }, fontSize = 11 };
            if (GUILayout.Button(new GUIContent(b.boneName + "  (" + b.component.GetType().Name + ")",
                "Cliquer pour selectionner l'objet dans la scene"),
                ns, GUILayout.ExpandWidth(true), GUILayout.Height(22)))
            {
                Selection.activeGameObject = b.component.gameObject;
                EditorGUIUtility.PingObject(b.component.gameObject);
                SceneView.FrameLastActiveSceneView();
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

            if (!b.isCollider)
                GUILayout.Label(b.childCount + " trsf", _styleSmall, GUILayout.Width(70));

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EndCard();
    }

    private void DrawPerfStats()
    {
        var pbs  = _phys.Where(b => !b.isCollider && b.keep).ToList();
        var cols = _phys.Where(b =>  b.isCollider  && b.keep).ToList();
        int pb = pbs.Count, col = cols.Count;
        int trsf = pbs.Sum(b => b.childCount + 1);
        int cc = pb * col * 2, cont = col;

        PerfRow("PhysBone Components", pb,   LIM_PB,   "Max : " + LIM_PB);
        PerfRow("PhysBone Transforms", trsf, LIM_TRSF, "Max : " + LIM_TRSF);
        PerfRow("PhysBone Colliders",  col,  LIM_COL,  "Max : " + LIM_COL);
        PerfRow("Collision Check Count", cc, LIM_CC,   "Max : " + LIM_CC);
        PerfRow("Contact Count",       cont, LIM_CONT, "Max : " + LIM_CONT);
    }

    private void PerfRow(string label, int val, int limit, string hint)
    {
        bool ok = val <= limit;
        EditorGUILayout.BeginHorizontal();
        var dotR = GUILayoutUtility.GetRect(14,14, GUILayout.Width(14), GUILayout.Height(20));
        EditorGUI.DrawRect(new Rect(dotR.x+1, dotR.y+4, 10, 10), ok ? COL_SUCCESS : COL_ERROR);
        GUILayout.Label(label + " : " + val + "  (" + hint + ")",
            new GUIStyle(EditorStyles.label){ normal = { textColor = ok ? COL_TEXT : COL_ERROR }, fontStyle = ok ? FontStyle.Normal : FontStyle.Bold });
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
    }

    private void ScanPhysics()
    {
        _phys.Clear();
        foreach (var c in _avatar.GetComponentsInChildren<Component>(true))
        {
            if (c == null) continue;
            string t = c.GetType().Name;
            if (t != "VRCPhysBone" && t != "VRCPhysBoneCollider") continue;
            int childCount = c.transform.GetComponentsInChildren<Transform>(true).Length - 1;
            _phys.Add(new PhysEntry { component=c, boneName=c.gameObject.name, isCollider=t=="VRCPhysBoneCollider", keep=true, childCount=childCount });
        }
        _physScanned = true;
        Log(LogLevel.Info, "Physics scan : " + _phys.Count(b=>!b.isCollider) + " PhysBones, " + _phys.Count(b=>b.isCollider) + " Colliders");
    }

    private void ApplyPhysRemovals()
    {
        Undo.RegisterFullObjectHierarchyUndo(GetTarget(), "Supprimer Physics");
        int n = 0;
        foreach (var b in _phys.Where(b => !b.keep && b.component != null))
        { Undo.DestroyObjectImmediate(b.component); n++; }
        SaveWorkingCopy();
        Log(LogLevel.Success, n + " composant(s) Physics supprime(s)");
        ScanPhysics();
    }

    // ═════════════════════════════════════════════════════════════
    //  TAB 2 — BONES (avec merge, duplicates, filter by SMR, vertex count)
    // ═════════════════════════════════════════════════════════════
    private void DrawBones()
    {
        if (_avatar == null) { InfoBox(T("msg.no.avatar.top")); return; }

        SectionLabel(T("bones.section"));
        InfoBox(T("bones.legend.info"));
        EditorGUILayout.Space(4);

        BeginCard();
        EditorGUILayout.BeginHorizontal();
        if (AccentBtn(new GUIContent(T("bones.scan.btn"),"Analyse tous les os de l'avatar"), GUILayout.Height(34), GUILayout.ExpandWidth(true))) ScanBones();
        if (SuccessBtn(new GUIContent(T("bones.merge.btn"),"Fusionne les os non utilises dans leur parent"), GUILayout.Height(34), GUILayout.Width(150))) MergeUnusedBones();
        if (SuccessBtn(new GUIContent(T("bones.dedup.btn"),"Supprime les os avec le meme nom au meme endroit"), GUILayout.Height(34), GUILayout.Width(160))) RemoveDuplicateBones();
        if (UndoBtn(new GUIContent(T("btn.undo"),"Ctrl+Z"), GUILayout.Height(34), GUILayout.Width(110))) { Undo.PerformUndo(); _bonesScanned = false; }
        EditorGUILayout.EndHorizontal();
        EndCard();

        if (!_bonesScanned) return;
        if (_boneList.Count == 0) { WarnBox(T("bones.none.found")); return; }

        EditorGUILayout.Space(6);
        int sk = _boneList.Count(b => b.usage == BoneUsage.Skinned);
        int pb = _boneList.Count(b => b.usage == BoneUsage.PhysBone);
        int st = _boneList.Count(b => b.usage == BoneUsage.Structural);
        int un = _boneList.Count(b => b.usage == BoneUsage.Unused);

        BeginCard();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(T("bones.skinned")     + sk, new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=COL_SUCCESS  }, fontStyle=FontStyle.Bold }, GUILayout.Width(90));
        GUILayout.Label(T("bones.physbones")   + pb, new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=COL_PBONE   }, fontStyle=FontStyle.Bold }, GUILayout.Width(100));
        GUILayout.Label(T("bones.structural")  + st, new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=COL_ACCENT2 }, fontStyle=FontStyle.Bold }, GUILayout.Width(100));
        GUILayout.Label(T("bones.unused")      + un, new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=COL_WARN    }, fontStyle=FontStyle.Bold }, GUILayout.Width(110));
        GUILayout.FlexibleSpace();
        GUILayout.Label(T("lbl.total") + _boneList.Count, new GUIStyle(EditorStyles.boldLabel){ normal={ textColor=COL_TEXT }, fontSize=12 });
        EditorGUILayout.EndHorizontal();
        EndCard();

        EditorGUILayout.Space(4);
        BeginCard();
        string[] filterLabels = {
            T("bones.filter.all")        + " (" + _boneList.Count + ")",
            T("bones.filter.skinned")    + " (" + sk + ")",
            T("bones.filter.physics")    + " (" + pb + ")",
            T("bones.filter.structural") + " (" + st + ")",
            T("bones.filter.unused")     + " (" + un + ")"
        };
        _boneFilter = GUILayout.Toolbar(_boneFilter, filterLabels);

        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(T("lbl.search"), new GUIStyle(EditorStyles.label){ normal={ textColor=COL_TEXT_DIM } }, GUILayout.Width(80));
        _boneSearch = EditorGUILayout.TextField(_boneSearch);
        if (SmallBtn("X")) _boneSearch = "";
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent(T("bones.filter.mesh"), "Affiche seulement les os utilises par ce SkinnedMeshRenderer"),
            new GUIStyle(EditorStyles.label){ normal={ textColor=COL_TEXT_DIM } }, GUILayout.Width(110));
        _boneMeshFilter = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(_boneMeshFilter, typeof(SkinnedMeshRenderer), true);
        if (SmallBtn("X")) _boneMeshFilter = null;
        EditorGUILayout.EndHorizontal();
        EndCard();

        var visible = _boneList.Where(b =>
            _boneFilter == 0 ||
            (_boneFilter == 1 && b.usage == BoneUsage.Skinned) ||
            (_boneFilter == 2 && b.usage == BoneUsage.PhysBone) ||
            (_boneFilter == 3 && b.usage == BoneUsage.Structural) ||
            (_boneFilter == 4 && b.usage == BoneUsage.Unused)
        ).Where(b =>
            string.IsNullOrEmpty(_boneSearch) || b.shortName.ToLower().Contains(_boneSearch.ToLower())
        ).Where(b =>
            _boneMeshFilter == null || b.ownerSMR == _boneMeshFilter
        ).ToList();

        EditorGUILayout.Space(4);
        BeginCard();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(T("lbl.results") + visible.Count + " / " + _boneList.Count, new GUIStyle(EditorStyles.boldLabel){ normal={ textColor=COL_TEXT } });
        GUILayout.FlexibleSpace();
        if (SmallBtn(T("btn.check.all")))              _boneList.ForEach(b => b.keep = true);
        if (SmallBtn(T("bones.uncheck.unused"))) _boneList.Where(b => b.usage == BoneUsage.Unused).ToList().ForEach(b => b.keep = false);
        EditorGUILayout.EndHorizontal();
        EndCard();

        EditorGUILayout.Space(4);
        BeginCard();
        _boneScroll = EditorGUILayout.BeginScrollView(_boneScroll, GUILayout.MaxHeight(380));
        int _boneRowIdx = 0;
        foreach (var b in visible)
        {
            if (b.bone == null) continue;
            DrawBoneRow(b, _boneRowIdx++);
        }
        EditorGUILayout.EndScrollView();
        EndCard();

        EditorGUILayout.Space(8);
        int toRemove = _boneList.Count(b => !b.keep);
        EditorGUI.BeginDisabledGroup(toRemove == 0);
        if (DangerBtn(toRemove > 0
            ? T("dlg.delete") + " " + toRemove + " " + T("stat.bones")
            : T("bones.none.found"), GUILayout.Height(36)))
        {
            if (EditorUtility.DisplayDialog(T("dlg.confirm"),
                string.Format(T("bones.delete.confirm"), toRemove), T("dlg.delete"), T("dlg.cancel")))
                ApplyBoneRemovals();
        }
        EditorGUI.EndDisabledGroup();
    }


    private void DrawBoneRow(BoneEntry b, int rowIdx = 0)
    {
        var row = EditorGUILayout.BeginHorizontal(GUILayout.Height(26));
        EditorGUI.DrawRect(row, rowIdx % 2 == 0 ? new Color(0,0,0,0) : new Color(1,1,1,0.04f));
        if (!b.keep) EditorGUI.DrawRect(row, new Color(COL_ERROR.r,COL_ERROR.g,COL_ERROR.b,0.12f));

        bool nk = EditorGUILayout.Toggle(b.keep, GUILayout.Width(18));
        if (nk != b.keep) { b.keep = nk; Repaint(); }

        int depth = Mathf.Min(b.depth, 8);
        GUILayout.Space(depth * 12);

        Color cat = UsageColor(b.usage);
        var dotR = GUILayoutUtility.GetRect(10,10, GUILayout.Width(10), GUILayout.Height(26));
        EditorGUI.DrawRect(new Rect(dotR.x, dotR.y+8, 10, 10), cat);
        if (b.hasPhysBone)
            EditorGUI.DrawRect(new Rect(dotR.x+2, dotR.y+10, 6, 6), COL_PBONE);

        Color tc = b.keep ? cat : COL_ERROR;
        var ns = new GUIStyle(_styleLink){ normal = { textColor = tc }, fontSize = 11, fixedHeight = 24 };
        if (GUILayout.Button(b.shortName, ns, GUILayout.ExpandWidth(true), GUILayout.Height(24)))
        {
            Selection.activeObject = b.bone;
            EditorGUIUtility.PingObject(b.bone);
            SceneView.FrameLastActiveSceneView();
        }
        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

        if (b.verticesInfluenced > 0)
            GUILayout.Label(b.verticesInfluenced + " vtx", _styleSmall, GUILayout.Width(70));
        else
            GUILayout.Label("0 vtx", _styleSmall, GUILayout.Width(70));

        GUILayout.Label(UsageLabel(b.usage), new GUIStyle(EditorStyles.miniLabel)
        { normal = { textColor = cat }, fontSize = 10, alignment = TextAnchor.MiddleRight }, GUILayout.Width(90));

        EditorGUILayout.EndHorizontal();
    }

    private Color UsageColor(BoneUsage u) => u switch
    {
        BoneUsage.Skinned    => COL_SUCCESS,
        BoneUsage.PhysBone   => COL_PBONE,
        BoneUsage.Structural => COL_ACCENT2,
        _                    => COL_WARN
    };

    private string UsageLabel(BoneUsage u) => u switch
    {
        BoneUsage.Skinned    => T("bones.usage.skinned"),
        BoneUsage.PhysBone   => T("bones.usage.physbone"),
        BoneUsage.Structural => T("bones.usage.structural"),
        _                    => T("bones.usage.unused")
    };

    private void ScanBones()
    {
        try
        {
            EditorUtility.DisplayProgressBar("Scanning Bones", "Finding skeleton roots...", 0f);
            _boneList.Clear();
            if (_avatar == null) return;

            var allTrsf = _avatar.GetComponentsInChildren<Transform>(true);
            var smrs    = _avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            var skeletonRoots = new HashSet<Transform>();
            foreach (var smr in smrs)
            {
                if (smr == null) continue;
                if (smr.rootBone != null) skeletonRoots.Add(smr.rootBone);
                if (smr.bones != null)
                    foreach (var b in smr.bones)
                        if (b != null)
                        {
                            var cur = b;
                            while (cur != null && cur.parent != null && cur.parent != _avatar.transform)
                                cur = cur.parent;
                            if (cur != null) skeletonRoots.Add(cur);
                        }
            }
            if (skeletonRoots.Count == 0)
            {
                foreach (var t in allTrsf)
                    if (t.name.ToLower().Contains("armature") || t.name.ToLower().Contains("hips"))
                    { skeletonRoots.Add(t); break; }
            }

            var allBones = new HashSet<Transform>();
            foreach (var root in skeletonRoots)
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.GetComponent<Renderer>() != null) continue;
                    allBones.Add(t);
                }
                allBones.Add(root);
            }
            if (allBones.Count == 0)
            {
                foreach (var t in allTrsf)
                {
                    if (t == _avatar.transform) continue;
                    if (t.GetComponent<Renderer>() != null) continue;
                    allBones.Add(t);
                }
            }

            // Compte vertices influences
            EditorUtility.DisplayProgressBar("Scanning Bones", "Counting bone influences...", 0.3f);
            var boneVertCount = new Dictionary<Transform, int>();
            var boneToSMR = new Dictionary<Transform, SkinnedMeshRenderer>();
            foreach (var smr in smrs)
            {
                if (smr == null || smr.bones == null || smr.sharedMesh == null) continue;
                var weights = smr.sharedMesh.boneWeights;
                var bones = smr.bones;
                foreach (var w in weights)
                {
                    void Add(int bi, float wt) {
                        if (wt <= 0 || bi >= bones.Length || bones[bi] == null) return;
                        if (!boneVertCount.ContainsKey(bones[bi])) { boneVertCount[bones[bi]] = 0; boneToSMR[bones[bi]] = smr; }
                        boneVertCount[bones[bi]]++;
                    }
                    Add(w.boneIndex0, w.weight0);
                    Add(w.boneIndex1, w.weight1);
                    Add(w.boneIndex2, w.weight2);
                    Add(w.boneIndex3, w.weight3);
                }
            }

            EditorUtility.DisplayProgressBar("Scanning Bones", "Identifying bone types...", 0.6f);
            // PhysBone = bone avec VRCPhysBone directement OU enfant d'une chaine VRCPhysBone
            var physBones = new HashSet<Transform>();
            foreach (var c in _avatar.GetComponentsInChildren<Component>(true))
            {
                if (c == null || c.GetType().Name != "VRCPhysBone") continue;
                Transform chainRoot = c.transform;
                var rtField = c.GetType().GetField("rootTransform");
                if (rtField != null) { var rt = rtField.GetValue(c) as Transform; if (rt != null) chainRoot = rt; }
                physBones.Add(chainRoot);
                foreach (var ch in chainRoot.GetComponentsInChildren<Transform>(true)) physBones.Add(ch);
            }

            var structural = new HashSet<Transform>();
            foreach (var b in boneVertCount.Keys.Concat(physBones))
            {
                var cur = b.parent;
                while (cur != null && cur != _avatar.transform && allBones.Contains(cur))
                {
                    structural.Add(cur);
                    cur = cur.parent;
                }
            }

            int MaxDepth(Transform t)
            {
                int d = 0;
                var cur = t.parent;
                while (cur != null && cur != _avatar.transform) { d++; cur = cur.parent; }
                return d;
            }

            foreach (var t in allBones.OrderBy(b => GetFullPath(b, _avatar.transform)))
            {
                BoneUsage usage;
                if (boneVertCount.ContainsKey(t)) usage = BoneUsage.Skinned;
                else if (physBones.Contains(t))   usage = BoneUsage.PhysBone;
                else if (structural.Contains(t))  usage = BoneUsage.Structural;
                else                              usage = BoneUsage.Unused;

                _boneList.Add(new BoneEntry
                {
                    bone        = t,
                    shortName   = t.name,
                    fullPath    = GetFullPath(t, _avatar.transform),
                    depth       = MaxDepth(t),
                    usage       = usage,
                    hasPhysBone = physBones.Contains(t),
                    verticesInfluenced = boneVertCount.ContainsKey(t) ? boneVertCount[t] : 0,
                    ownerSMR    = boneToSMR.ContainsKey(t) ? boneToSMR[t] : null,
                    keep        = true
                });
            }

            _bonesScanned = true;
            Log(LogLevel.Info, _boneList.Count + " os scannes, " + _boneList.Count(b=>b.usage==BoneUsage.Unused) + " non utilises");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private string GetFullPath(Transform t, Transform root)
    {
        string path = t.name;
        var cur = t.parent;
        while (cur != null && cur != root) { path = cur.name + "/" + path; cur = cur.parent; }
        return path;
    }

    private void ApplyBoneRemovals()
    {
        Undo.RegisterFullObjectHierarchyUndo(GetTarget(), "Supprimer os");
        int n = 0;
        foreach (var b in _boneList.Where(b => !b.keep && b.bone != null).OrderByDescending(b => b.depth))
        {
            if (b.bone != null)
            { Undo.DestroyObjectImmediate(b.bone.gameObject); n++; }
        }
        SaveWorkingCopy();
        Log(LogLevel.Success, n + " os supprime(s)");
        ScanBones();
    }

    private void MergeUnusedBones()
    {
        if (_boneList.Count == 0) { WarnBox("Scan requis."); return; }
        if (!EditorUtility.DisplayDialog("Fusionner os non utilises",
            "Chaque os non utilise sera fusionne dans son parent (ses enfants deviennent enfants du parent).\nCtrl+Z pour annuler.", "Fusionner","Annuler")) return;

        Undo.RegisterFullObjectHierarchyUndo(GetTarget(), "Merge unused bones");
        int n = 0;
        foreach (var b in _boneList.Where(b => b.usage == BoneUsage.Unused && b.bone != null).OrderByDescending(b => b.depth))
        {
            if (b.bone == null || b.bone.parent == null) continue;
            var parent = b.bone.parent;
            for (int i = b.bone.childCount - 1; i >= 0; i--)
                b.bone.GetChild(i).SetParent(parent, true);
            Undo.DestroyObjectImmediate(b.bone.gameObject);
            n++;
        }
        Log(LogLevel.Success, n + " os non utilises fusionnes dans leur parent");
        ScanBones();
    }

    private void RemoveDuplicateBones()
    {
        if (_boneList.Count == 0) { WarnBox("Scan requis."); return; }
        if (!EditorUtility.DisplayDialog("Supprimer doublons",
            "Supprime les os qui ont le meme nom et le meme parent (doublons).", "Supprimer","Annuler")) return;
        Undo.RegisterFullObjectHierarchyUndo(GetTarget(), "Remove duplicate bones");
        var groups = _boneList.Where(b => b.bone != null)
            .GroupBy(b => (b.bone.parent != null ? b.bone.parent.GetInstanceID() : 0) + "/" + b.shortName)
            .Where(g => g.Count() > 1);
        int n = 0;
        foreach (var g in groups)
        {
            foreach (var b in g.Skip(1))
            {
                if (b.bone != null) { Undo.DestroyObjectImmediate(b.bone.gameObject); n++; }
            }
        }
        Log(LogLevel.Success, n + " os dupliques supprimes");
        ScanBones();
    }

    // ═════════════════════════════════════════════════════════════
    //  TAB 3 — MESH & UV
    // ═════════════════════════════════════════════════════════════
    private void DrawMeshUV()
    {
        if (_avatar == null) { InfoBox(T("msg.no.avatar.top")); return; }

        SectionLabel(T("mesh.section"));
        BeginCard();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Compression", "Off = aucune | Low = légère | Medium = équilibrée | High = maximum"), GUILayout.Width(90));
        _compressionLvl = EditorGUILayout.IntSlider(_compressionLvl, 0, 3);
        EditorGUILayout.LabelField(_compressionLvlLabels[_compressionLvl], GUILayout.Width(54));
        EditorGUILayout.EndHorizontal();
        EndCard();

        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        if (AccentBtn(new GUIContent(T("mesh.apply.btn"),"Applique la compression aux meshes"), GUILayout.Height(30), GUILayout.ExpandWidth(true))) ApplyMeshCompression();
        if (UndoBtn(T("btn.undo"), GUILayout.Height(30), GUILayout.Width(90))) Undo.PerformUndo();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(14);
        DrawMeshCombining();
        EditorGUILayout.Space(14);
        DrawUVPainter();
    }

    private void DrawMeshCombining()
    {
        SectionLabel(T("mesh.combine.section"));
        InfoBox(T("mesh.combine.info"));
        BeginCard();

        EditorGUILayout.BeginHorizontal();
        if (SmallBtn(T("mesh.add.smr"))) _meshMergeTargets.Add(null);
        if (SmallBtn(T("btn.clear"))) _meshMergeTargets.Clear();
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < _meshMergeTargets.Count; i++)
        {
            var _mr = EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
            EditorGUI.DrawRect(_mr, i % 2 == 0 ? new Color(0,0,0,0) : new Color(1,1,1,0.04f));
            _meshMergeTargets[i] = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(_meshMergeTargets[i], typeof(SkinnedMeshRenderer), true);
            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                _meshMergeTargets.RemoveAt(i); i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        EndCard();

        int totalBS = _meshMergeTargets.Where(m => m != null && m.sharedMesh != null).Sum(m => m.sharedMesh.blendShapeCount);
        if (totalBS > 0)
        {
            EditorGUILayout.Space(4);
            WarnBox("\u26a0\ufe0f " + totalBS + " blend shape(s) detectes. Apres la combinaison, tous les blend shapes du mesh resultant seront a 0 — pensez a les reinitialiser dans l'Animator ou manuellement dans l'Inspector.");
        }

        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(_meshMergeTargets.Count(m => m != null) < 2);
        if (AccentBtn(new GUIContent(T("mesh.combine.btn"), "Fusionne tous les meshes en un seul"), GUILayout.Height(30))) CombineSelectedMeshes();
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
    }

    private void CombineSelectedMeshes()
    {
        var targets = _meshMergeTargets.Where(m => m != null && m.sharedMesh != null).ToList();
        if (targets.Count < 2) { WarnBox("Au moins 2 meshes requis."); return; }

        string msg = "Combiner " + targets.Count + " meshes en un seul SkinnedMeshRenderer ?\nLes originaux seront desactives. Ctrl+Z pour annuler.";
        if (!EditorUtility.DisplayDialog("Combiner", msg, "Combiner", "Annuler")) return;

        try
        {
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Collect unique bones + bindposes
            var allBones     = new List<Transform>();
            var allBindposes = new List<Matrix4x4>();
            foreach (var smr in targets)
            {
                var bones = smr.bones;
                var bp    = smr.sharedMesh.bindposes;
                for (int b = 0; b < bones.Length; b++)
                {
                    if (bones[b] != null && !allBones.Contains(bones[b]))
                    {
                        allBones.Add(bones[b]);
                        allBindposes.Add(b < bp.Length ? bp[b] : Matrix4x4.identity);
                    }
                }
            }

            // 2. Manually accumulate per-mesh data (avoids CombineMeshes bone-index offset bug)
            var verts    = new List<Vector3>();
            var norms    = new List<Vector3>();
            var tangs    = new List<Vector4>();
            var uv0List  = new List<Vector2>();
            var uv1List  = new List<Vector2>();
            var bwList   = new List<BoneWeight>();
            var subTris  = new List<int[]>();
            var allMats  = new List<Material>();
            bool hasUV2  = false;
            int vertOffset = 0;

            foreach (var smr in targets)
            {
                var mesh  = smr.sharedMesh;
                var bones = smr.bones;

                var remap = new int[bones.Length];
                for (int b = 0; b < bones.Length; b++)
                    remap[b] = bones[b] != null ? allBones.IndexOf(bones[b]) : 0;

                verts.AddRange(mesh.vertices);

                var n = mesh.normals;
                norms.AddRange(n.Length > 0 ? n : new Vector3[mesh.vertexCount]);
                var t = mesh.tangents;
                tangs.AddRange(t.Length > 0 ? t : new Vector4[mesh.vertexCount]);
                var u0 = mesh.uv;
                uv0List.AddRange(u0.Length > 0 ? u0 : new Vector2[mesh.vertexCount]);
                var u1 = mesh.uv2;
                if (u1.Length > 0) hasUV2 = true;
                uv1List.AddRange(u1.Length > 0 ? u1 : new Vector2[mesh.vertexCount]);

                var bw = mesh.boneWeights.Length > 0 ? mesh.boneWeights : new BoneWeight[mesh.vertexCount];
                foreach (var w in bw)
                {
                    bwList.Add(new BoneWeight
                    {
                        boneIndex0 = w.boneIndex0 < remap.Length ? remap[w.boneIndex0] : 0,
                        boneIndex1 = w.boneIndex1 < remap.Length ? remap[w.boneIndex1] : 0,
                        boneIndex2 = w.boneIndex2 < remap.Length ? remap[w.boneIndex2] : 0,
                        boneIndex3 = w.boneIndex3 < remap.Length ? remap[w.boneIndex3] : 0,
                        weight0 = w.weight0, weight1 = w.weight1,
                        weight2 = w.weight2, weight3 = w.weight3
                    });
                }

                var mats = smr.sharedMaterials;
                for (int s = 0; s < mesh.subMeshCount; s++)
                {
                    var tris = mesh.GetTriangles(s);
                    for (int i = 0; i < tris.Length; i++) tris[i] += vertOffset;
                    subTris.Add(tris);
                    allMats.Add(s < mats.Length ? mats[s] : null);
                }
                vertOffset += mesh.vertexCount;
            }

            // 3. Build combined Mesh
            var newMesh = new Mesh { name = "CombinedMesh" };
            newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            newMesh.SetVertices(verts);
            newMesh.SetNormals(norms);
            newMesh.SetTangents(tangs);
            newMesh.SetUVs(0, uv0List);
            if (hasUV2) newMesh.SetUVs(1, uv1List);
            newMesh.boneWeights  = bwList.ToArray();
            newMesh.bindposes    = allBindposes.ToArray();
            newMesh.subMeshCount = subTris.Count;
            for (int s = 0; s < subTris.Count; s++) newMesh.SetTriangles(subTris[s], s);

            // Blend shapes: collect all unique names, zero-pad vertices from other meshes
            var meshVertOffsets = new int[targets.Count];
            int vOff2 = 0;
            for (int mi = 0; mi < targets.Count; mi++) { meshVertOffsets[mi] = vOff2; vOff2 += targets[mi].sharedMesh.vertexCount; }
            int totalVerts = vOff2;

            var bsNames   = new List<string>();
            var bsSources = new Dictionary<string, List<(int mi, int bi)>>();
            for (int mi = 0; mi < targets.Count; mi++)
            {
                var mesh = targets[mi].sharedMesh;
                for (int bi = 0; bi < mesh.blendShapeCount; bi++)
                {
                    string bsn = mesh.GetBlendShapeName(bi);
                    if (!bsNames.Contains(bsn)) bsNames.Add(bsn);
                    if (!bsSources.ContainsKey(bsn)) bsSources[bsn] = new List<(int, int)>();
                    bsSources[bsn].Add((mi, bi));
                }
            }

            var dV = new Vector3[totalVerts];
            var dN = new Vector3[totalVerts];
            var dT = new Vector3[totalVerts];
            foreach (string bsn in bsNames)
            {
                int maxFrames = 1;
                foreach (var (mi, bi) in bsSources[bsn])
                    maxFrames = Mathf.Max(maxFrames, targets[mi].sharedMesh.GetBlendShapeFrameCount(bi));

                for (int frame = 0; frame < maxFrames; frame++)
                {
                    System.Array.Clear(dV, 0, totalVerts);
                    System.Array.Clear(dN, 0, totalVerts);
                    System.Array.Clear(dT, 0, totalVerts);
                    float weight = 100f;

                    foreach (var (mi, bi) in bsSources[bsn])
                    {
                        var mesh2 = targets[mi].sharedMesh;
                        int actualFrame = Mathf.Min(frame, mesh2.GetBlendShapeFrameCount(bi) - 1);
                        weight = mesh2.GetBlendShapeFrameWeight(bi, actualFrame);
                        int vc = mesh2.vertexCount;
                        var fdV = new Vector3[vc]; var fdN = new Vector3[vc]; var fdT = new Vector3[vc];
                        mesh2.GetBlendShapeFrameVertices(bi, actualFrame, fdV, fdN, fdT);
                        int baseV = meshVertOffsets[mi];
                        for (int v = 0; v < vc; v++) { dV[baseV+v] = fdV[v]; dN[baseV+v] = fdN[v]; dT[baseV+v] = fdT[v]; }
                    }
                    newMesh.AddBlendShapeFrame(bsn, weight, dV, dN, dT);
                }
            }

            newMesh.RecalculateBounds();

            // 4. Save asset
            string outDir = "Assets/OptimizedMeshes";
            EnsureFolder(outDir);
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(outDir + "/CombinedMesh.asset");
            AssetDatabase.CreateAsset(newMesh, assetPath);

            // 5. Create SkinnedMeshRenderer
            var go = new GameObject("CombinedMesh");
            go.transform.SetParent(_avatar.transform, false);
            Undo.RegisterCreatedObjectUndo(go, "Combine Meshes");

            var newSmr = go.AddComponent<SkinnedMeshRenderer>();
            newSmr.sharedMesh      = newMesh;
            newSmr.bones           = allBones.ToArray();
            newSmr.rootBone        = targets[0].rootBone ?? targets[0].transform;
            newSmr.sharedMaterials = allMats.ToArray();

            // 6. Disable originals
            foreach (var smr in targets)
            {
                Undo.RecordObject(smr.gameObject, "Combine Meshes");
                smr.gameObject.SetActive(false);
            }

            Undo.CollapseUndoOperations(undoGroup);
            AssetDatabase.SaveAssets();
            Log(LogLevel.Success, targets.Count + " meshes combines → " + assetPath + " (" + allBones.Count + " bones, " + allMats.Count + " materiaux)");
        }
        catch (Exception e)
        {
            Log(LogLevel.Error, "Combine failed : " + e.Message);
        }
    }

    private void DrawHiddenPolyDetection()
    {
        SectionLabel(T("mesh.hidden.section"));
        InfoBox(T("mesh.hidden.info"));
        BeginCard();

        EditorGUILayout.BeginHorizontal();
        if (AccentBtn(new GUIContent(T("mesh.hidden.detect.btn"), "Utilise raycast pour detecter les triangles caches"), GUILayout.Height(28), GUILayout.ExpandWidth(true))) DetectHiddenPolygons();
        EditorGUILayout.EndHorizontal();

        if (_hiddenTriangles.Count > 0)
        {
            EditorGUILayout.Space(4);
            GUILayout.Label(_hiddenTriangles.Count + " triangles caches detectes", new GUIStyle(EditorStyles.boldLabel){ normal={textColor=COL_WARN} });
            if (DangerBtn("Supprimer les " + _hiddenTriangles.Count + " triangles caches", GUILayout.Height(28)))
            {
                if (EditorUtility.DisplayDialog("Confirmation",
                    "Supprimer " + _hiddenTriangles.Count + " triangles ?", "Supprimer","Annuler"))
                    RemoveHiddenTriangles();
            }
        }
        EndCard();
    }

    private void DetectHiddenPolygons()
    {
        if (_uvTargetSMR == null) { WarnBox("Selectionne un mesh dans la section UV Painter."); return; }
        var mesh = _uvTargetSMR.sharedMesh;
        if (mesh == null) return;

        _hiddenTriangles.Clear();
        var verts = mesh.vertices;
        var normals = mesh.normals;
        var tris = mesh.triangles;

        // Heuristique simple : triangle est cache si son centre est tres proche d'un autre triangle oppose
        var bounds = mesh.bounds;
        float threshold = bounds.size.magnitude * 0.02f;

        try
        {
            EditorUtility.DisplayProgressBar("Detection polygones caches", "Analyse...", 0f);
            for (int i = 0; i + 2 < tris.Length; i += 3)
            {
                if (i % 300 == 0) EditorUtility.DisplayProgressBar("Detection polygones caches", (i/3) + " / " + (tris.Length/3), (float)i/tris.Length);
                Vector3 v0 = verts[tris[i]], v1 = verts[tris[i+1]], v2 = verts[tris[i+2]];
                Vector3 center = (v0 + v1 + v2) / 3f;
                Vector3 triNormal = Vector3.Cross(v1 - v0, v2 - v0).normalized;

                // Cherche un triangle oppose proche avec normale inversee
                for (int j = 0; j + 2 < tris.Length; j += 3)
                {
                    if (i == j) continue;
                    Vector3 u0 = verts[tris[j]], u1 = verts[tris[j+1]], u2 = verts[tris[j+2]];
                    Vector3 uc = (u0 + u1 + u2) / 3f;
                    if (Vector3.Distance(center, uc) > threshold) continue;
                    Vector3 uN = Vector3.Cross(u1 - u0, u2 - u0).normalized;
                    if (Vector3.Dot(triNormal, uN) < -0.7f)
                    {
                        _hiddenTriangles.Add(i / 3);
                        break;
                    }
                }
            }
        }
        finally { EditorUtility.ClearProgressBar(); }

        Log(LogLevel.Info, _hiddenTriangles.Count + " triangles caches detectes");
    }

    private void RemoveHiddenTriangles()
    {
        if (_uvTargetSMR == null || _uvTargetSMR.sharedMesh == null) return;
        var mesh = _uvTargetSMR.sharedMesh;
        var tris = mesh.triangles.ToList();
        var sorted = _hiddenTriangles.OrderByDescending(t => t).ToList();
        foreach (int t in sorted)
        {
            int idx = t * 3;
            if (idx + 2 < tris.Count) { tris.RemoveAt(idx+2); tris.RemoveAt(idx+1); tris.RemoveAt(idx); }
        }
        Undo.RecordObject(mesh, "Remove hidden triangles");
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        Log(LogLevel.Success, sorted.Count + " triangles caches supprimes");
        _hiddenTriangles.Clear();
    }

    private void DrawUVPainter()
    {
        SectionLabel(T("mesh.uv.section"));
        InfoBox(T("mesh.uv.info"));

        BeginCard();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(T("lbl.mesh"), new GUIStyle(EditorStyles.label){normal={textColor=COL_TEXT_DIM}}, GUILayout.Width(50));
        var prevSMR = _uvTargetSMR;
        _uvTargetSMR = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(_uvTargetSMR, typeof(SkinnedMeshRenderer), true);
        if (_uvTargetSMR != prevSMR) GenerateUVPreview();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(T("lbl.tool"), new GUIStyle(EditorStyles.label){normal={textColor=COL_TEXT_DIM}}, GUILayout.Width(50));
        string[] modes = { T("uv.tool.brush"), T("uv.tool.rect"), T("uv.tool.lasso") };
        for (int i = 0; i < 3; i++)
        {
            bool active = (int)_uvMode == i;
            var s = new GUIStyle(GUI.skin.button)
            {
                normal = { background = MakeTex(1,1, active ? COL_ACCENT : COL_BG3), textColor = active ? Color.white : COL_TEXT_DIM },
                fontStyle = active ? FontStyle.Bold : FontStyle.Normal, fontSize = 10
            };
            if (GUILayout.Button(modes[i], s)) _uvMode = (UVPaintMode)i;
        }
        EditorGUILayout.EndHorizontal();

        if (_uvMode == UVPaintMode.Brush)
        {
            _uvBrushSize = EditorGUILayout.IntSlider(T("uv.brush.size"), _uvBrushSize, 2, 80);
            _uvBrushOpacity = EditorGUILayout.Slider(T("uv.opacity"), _uvBrushOpacity, 0.1f, 1f);
        }
        EndCard();

        EditorGUILayout.Space(4);
        if (_uvTargetSMR != null && _uvPreviewTex != null)
        {
            EditorGUILayout.BeginHorizontal();
            _uvPaintActive = GUILayout.Toggle(_uvPaintActive,
                _uvPaintActive ? T("uv.paint.active") : T("uv.paint.start"),
                _uvPaintActive ? _styleDangerBtn : _styleAccentBtn, GUILayout.Height(28));
            if (SmallBtn("↩️ Undo peinture") && _uvUndoStack.Count > 0)
            {
                _uvRedoStack.Push(new HashSet<int>(_markedTriangles));
                _markedTriangles = _uvUndoStack.Pop().ToList();
                GenerateUVPreview(); RepaintMarkedOnTex();
            }
            if (SmallBtn("↪️ Redo") && _uvRedoStack.Count > 0)
            {
                _uvUndoStack.Push(new HashSet<int>(_markedTriangles));
                _markedTriangles = _uvRedoStack.Pop().ToList();
                GenerateUVPreview(); RepaintMarkedOnTex();
            }
            if (SmallBtn("🧽 Effacer"))
            {
                _uvUndoStack.Push(new HashSet<int>(_markedTriangles));
                _markedTriangles.Clear();
                GenerateUVPreview();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            _uvScroll = EditorGUILayout.BeginScrollView(_uvScroll, GUILayout.Height(UV_TEX_SIZE + 10));
            var uvRect = GUILayoutUtility.GetRect(UV_TEX_SIZE, UV_TEX_SIZE, GUILayout.Width(UV_TEX_SIZE), GUILayout.Height(UV_TEX_SIZE));
            GUI.DrawTexture(uvRect, _uvPreviewTex, ScaleMode.StretchToFill);

            HandleUVInput(uvRect);

            // Dessine selection lasso/rect en cours
            if (_uvMode == UVPaintMode.Rectangle && _rectStart.HasValue && _uvPaintActive)
            {
                Vector2 cur = Event.current.mousePosition;
                var r = new Rect(Mathf.Min(_rectStart.Value.x, cur.x), Mathf.Min(_rectStart.Value.y, cur.y),
                    Mathf.Abs(cur.x - _rectStart.Value.x), Mathf.Abs(cur.y - _rectStart.Value.y));
                EditorGUI.DrawRect(r, new Color(1,0.2f,0.2f,0.25f));
            }
            if (_uvMode == UVPaintMode.Lasso && _lassoPoints.Count > 1 && _uvPaintActive)
            {
                Handles.BeginGUI();
                Handles.color = new Color(1,0.3f,0.3f,0.9f);
                for (int i = 0; i < _lassoPoints.Count - 1; i++)
                    Handles.DrawAAPolyLine(2f, _lassoPoints[i], _lassoPoints[i+1]);
                Handles.EndGUI();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4);
            int marked = _markedTriangles.Count;
            int totalTris = _uvTargetSMR.sharedMesh != null ? _uvTargetSMR.sharedMesh.triangles.Length / 3 : 0;

            BeginCard();
            EditorGUILayout.BeginHorizontal();
            MiniStatTile(T("uv.marked.tris"), marked.ToString(), marked > 0 ? COL_WARN : COL_TEXT_DIM);
            MiniStatTile(T("uv.total.tris"), totalTris.ToString(), COL_ACCENT2);
            EditorGUILayout.EndHorizontal();
            EndCard();

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(marked == 0);
            if (DangerBtn("Supprimer " + marked + " triangle(s)", GUILayout.Height(32), GUILayout.ExpandWidth(true)))
            {
                if (EditorUtility.DisplayDialog("Confirmation",
                    "Supprimer " + marked + " triangle(s) ?\nSauvegarde ton projet avant.", "Supprimer","Annuler"))
                    ApplyUVPaintDeletion();
            }
            EditorGUI.EndDisabledGroup();
            if (UndoBtn("Undo Unity", GUILayout.Height(32), GUILayout.Width(100))) Undo.PerformUndo();
            EditorGUILayout.EndHorizontal();
        }
    }

    private void HandleUVInput(Rect uvRect)
    {
        if (!_uvPaintActive) return;
        var e = Event.current;

        if (_uvMode == UVPaintMode.Brush)
        {
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && uvRect.Contains(e.mousePosition))
            {
                if (e.type == EventType.MouseDown) _uvUndoStack.Push(new HashSet<int>(_markedTriangles));
                float nx = (e.mousePosition.x - uvRect.x) / uvRect.width;
                float ny = 1f - (e.mousePosition.y - uvRect.y) / uvRect.height;
                PaintUV(nx, ny);
                e.Use(); Repaint();
            }
        }
        else if (_uvMode == UVPaintMode.Rectangle)
        {
            if (e.type == EventType.MouseDown && uvRect.Contains(e.mousePosition))
            {
                _rectStart = e.mousePosition;
                _uvUndoStack.Push(new HashSet<int>(_markedTriangles));
                e.Use();
            }
            else if (e.type == EventType.MouseUp && _rectStart.HasValue)
            {
                float nx0 = (_rectStart.Value.x - uvRect.x) / uvRect.width;
                float ny0 = 1f - (_rectStart.Value.y - uvRect.y) / uvRect.height;
                float nx1 = (e.mousePosition.x - uvRect.x) / uvRect.width;
                float ny1 = 1f - (e.mousePosition.y - uvRect.y) / uvRect.height;
                MarkRect(Mathf.Min(nx0,nx1), Mathf.Min(ny0,ny1), Mathf.Max(nx0,nx1), Mathf.Max(ny0,ny1));
                _rectStart = null;
                GenerateUVPreview(); RepaintMarkedOnTex();
                e.Use();
            }
            else if (e.type == EventType.MouseDrag) { e.Use(); Repaint(); }
        }
        else if (_uvMode == UVPaintMode.Lasso)
        {
            if (e.type == EventType.MouseDown && uvRect.Contains(e.mousePosition))
            {
                _lassoPoints.Clear();
                _lassoPoints.Add(e.mousePosition);
                _uvUndoStack.Push(new HashSet<int>(_markedTriangles));
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _lassoPoints.Count > 0)
            {
                _lassoPoints.Add(e.mousePosition);
                e.Use(); Repaint();
            }
            else if (e.type == EventType.MouseUp && _lassoPoints.Count > 2)
            {
                MarkLasso(uvRect);
                _lassoPoints.Clear();
                GenerateUVPreview(); RepaintMarkedOnTex();
                e.Use();
            }
        }
    }

    private void MarkRect(float nx0, float ny0, float nx1, float ny1)
    {
        if (_uvTargetSMR == null || _uvTargetSMR.sharedMesh == null) return;
        var uv = _uvTargetSMR.sharedMesh.uv;
        var tris = _uvTargetSMR.sharedMesh.triangles;
        for (int i = 0; i + 2 < tris.Length; i += 3)
        {
            int t = i / 3;
            if (_markedTriangles.Contains(t)) continue;
            for (int k = 0; k < 3; k++)
            {
                int vi = tris[i+k];
                if (vi >= uv.Length) continue;
                if (uv[vi].x >= nx0 && uv[vi].x <= nx1 && uv[vi].y >= ny0 && uv[vi].y <= ny1)
                { _markedTriangles.Add(t); break; }
            }
        }
    }

    private void MarkLasso(Rect uvRect)
    {
        if (_uvTargetSMR == null || _uvTargetSMR.sharedMesh == null) return;
        var uv = _uvTargetSMR.sharedMesh.uv;
        var tris = _uvTargetSMR.sharedMesh.triangles;

        bool PointInPolygon(Vector2 p)
        {
            int n = _lassoPoints.Count; bool inside = false;
            for (int i = 0, j = n - 1; i < n; j = i++)
                if (((_lassoPoints[i].y > p.y) != (_lassoPoints[j].y > p.y)) &&
                    (p.x < (_lassoPoints[j].x - _lassoPoints[i].x) * (p.y - _lassoPoints[i].y) / (_lassoPoints[j].y - _lassoPoints[i].y) + _lassoPoints[i].x))
                    inside = !inside;
            return inside;
        }

        for (int i = 0; i + 2 < tris.Length; i += 3)
        {
            int t = i / 3;
            if (_markedTriangles.Contains(t)) continue;
            for (int k = 0; k < 3; k++)
            {
                int vi = tris[i+k];
                if (vi >= uv.Length) continue;
                float sx = uvRect.x + uv[vi].x * uvRect.width;
                float sy = uvRect.y + (1f - uv[vi].y) * uvRect.height;
                if (PointInPolygon(new Vector2(sx, sy)))
                { _markedTriangles.Add(t); break; }
            }
        }
    }

    private void RepaintMarkedOnTex()
    {
        if (_uvPreviewTex == null || _uvTargetSMR == null || _uvTargetSMR.sharedMesh == null) return;
        var uv = _uvTargetSMR.sharedMesh.uv;
        var tris = _uvTargetSMR.sharedMesh.triangles;
        foreach (int t in _markedTriangles)
        {
            int i = t * 3;
            if (i + 2 >= tris.Length) continue;
            for (int k = 0; k < 3; k++)
            {
                int vi = tris[i + k];
                if (vi >= uv.Length) continue;
                int cx = Mathf.RoundToInt(uv[vi].x * UV_TEX_SIZE);
                int cy = Mathf.RoundToInt(uv[vi].y * UV_TEX_SIZE);
                for (int dy = -3; dy <= 3; dy++)
                for (int dx = -3; dx <= 3; dx++)
                {
                    int px = cx+dx, py = cy+dy;
                    if (px<0||px>=UV_TEX_SIZE||py<0||py>=UV_TEX_SIZE) continue;
                    _uvPreviewTex.SetPixel(px, py, new Color(1f,0.2f,0.2f,1f));
                }
            }
        }
        _uvPreviewTex.Apply();
    }

    private void GenerateUVPreview()
    {
        if (_uvTargetSMR == null || _uvTargetSMR.sharedMesh == null)
        { _uvPreviewTex = null; _uvMaskTex = null; return; }

        var mesh = _uvTargetSMR.sharedMesh;
        var uv = mesh.uv;
        var tris = mesh.triangles;

        _uvPreviewTex = new Texture2D(UV_TEX_SIZE, UV_TEX_SIZE, TextureFormat.RGBA32, false);
        _uvPreviewTex.filterMode = FilterMode.Point;
        var px = new Color[UV_TEX_SIZE * UV_TEX_SIZE];
        for (int i = 0; i < px.Length; i++) px[i] = new Color(0.10f,0.10f,0.18f,1f);
        for (int i = 0; i + 2 < tris.Length; i += 3)
        {
            if (tris[i] >= uv.Length || tris[i+1] >= uv.Length || tris[i+2] >= uv.Length) continue;
            Vector2 a = uv[tris[i]], b = uv[tris[i+1]], c = uv[tris[i+2]];
            DrawLine(px, a, b, COL_ACCENT, UV_TEX_SIZE);
            DrawLine(px, b, c, COL_ACCENT, UV_TEX_SIZE);
            DrawLine(px, c, a, COL_ACCENT, UV_TEX_SIZE);
        }
        _uvPreviewTex.SetPixels(px);
        _uvPreviewTex.Apply();

        _uvMaskTex = new Texture2D(UV_TEX_SIZE, UV_TEX_SIZE, TextureFormat.RGBA32, false);
        var blank = new Color[UV_TEX_SIZE * UV_TEX_SIZE];
        for (int i = 0; i < blank.Length; i++) blank[i] = new Color(0,0,0,0);
        _uvMaskTex.SetPixels(blank); _uvMaskTex.Apply();
    }

    private void DrawLine(Color[] px, Vector2 a, Vector2 b, Color col, int size)
    {
        int x0 = Mathf.RoundToInt(Mathf.Clamp01(a.x) * (size-1));
        int y0 = Mathf.RoundToInt(Mathf.Clamp01(a.y) * (size-1));
        int x1 = Mathf.RoundToInt(Mathf.Clamp01(b.x) * (size-1));
        int y1 = Mathf.RoundToInt(Mathf.Clamp01(b.y) * (size-1));
        int dx = Mathf.Abs(x1-x0), dy = Mathf.Abs(y1-y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1, err = dx - dy;
        for (int s = 0; s < 2000; s++)
        {
            if (x0 >= 0 && x0 < size && y0 >= 0 && y0 < size)
                px[y0 * size + x0] = Color.Lerp(px[y0*size+x0], col, 0.85f);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2*err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 <  dx) { err += dx; y0 += sy; }
        }
    }

    private void PaintUV(float nx, float ny)
    {
        if (_uvPreviewTex == null) return;
        int cx = Mathf.RoundToInt(nx * UV_TEX_SIZE);
        int cy = Mathf.RoundToInt(ny * UV_TEX_SIZE);
        int r = _uvBrushSize;
        for (int py = cy-r; py <= cy+r; py++)
        for (int pxi = cx-r; pxi <= cx+r; pxi++)
        {
            if (pxi < 0 || pxi >= UV_TEX_SIZE || py < 0 || py >= UV_TEX_SIZE) continue;
            float d = Mathf.Sqrt((pxi-cx)*(pxi-cx)+(py-cy)*(py-cy));
            if (d > r) continue;
            float falloff = 1f - Mathf.Pow(d/r, 2f);
            float alpha = Mathf.Clamp01(falloff * _uvBrushOpacity);
            var cur = _uvPreviewTex.GetPixel(pxi, py);
            _uvPreviewTex.SetPixel(pxi, py, Color.Lerp(cur, new Color(1f,0.15f,0.15f,1f), alpha));
        }
        _uvPreviewTex.Apply();

        if (_uvTargetSMR == null || _uvTargetSMR.sharedMesh == null) return;
        var mesh = _uvTargetSMR.sharedMesh;
        var uv = mesh.uv; var tris = mesh.triangles;
        for (int i = 0; i + 2 < tris.Length; i += 3)
        {
            int t = i / 3;
            if (_markedTriangles.Contains(t)) continue;
            for (int k = 0; k < 3; k++)
            {
                int vi = tris[i+k];
                if (vi >= uv.Length) continue;
                int uvx = Mathf.RoundToInt(uv[vi].x * UV_TEX_SIZE);
                int uvy = Mathf.RoundToInt(uv[vi].y * UV_TEX_SIZE);
                float dist = Mathf.Sqrt((uvx-cx)*(uvx-cx)+(uvy-cy)*(uvy-cy));
                if (dist <= r) { _markedTriangles.Add(t); break; }
            }
        }
    }

    private void ApplyUVPaintDeletion()
    {
        if (_uvTargetSMR == null || _uvTargetSMR.sharedMesh == null) return;
        var orig = _uvTargetSMR.sharedMesh;
        var markedSet = new HashSet<int>(_markedTriangles);

        // Build new mesh as a full copy of the original
        var newMesh = new Mesh { name = orig.name + "_cut", indexFormat = orig.indexFormat };
        newMesh.vertices  = orig.vertices;
        newMesh.normals   = orig.normals;
        newMesh.tangents  = orig.tangents;
        newMesh.uv        = orig.uv;
        newMesh.uv2       = orig.uv2;
        newMesh.colors    = orig.colors;
        newMesh.bindposes = orig.bindposes;
        newMesh.boneWeights = orig.boneWeights;

        // Copy blendshapes
        for (int bs = 0; bs < orig.blendShapeCount; bs++)
        {
            string bsName = orig.GetBlendShapeName(bs);
            for (int fr = 0; fr < orig.GetBlendShapeFrameCount(bs); fr++)
            {
                float w = orig.GetBlendShapeFrameWeight(bs, fr);
                var dV = new Vector3[orig.vertexCount];
                var dN = new Vector3[orig.vertexCount];
                var dT = new Vector3[orig.vertexCount];
                orig.GetBlendShapeFrameVertices(bs, fr, dV, dN, dT);
                newMesh.AddBlendShapeFrame(bsName, w, dV, dN, dT);
            }
        }

        // Remove marked triangles per submesh
        newMesh.subMeshCount = orig.subMeshCount;
        int globalOffset = 0, removedCount = 0;
        for (int sub = 0; sub < orig.subMeshCount; sub++)
        {
            var origSub = orig.GetTriangles(sub);
            var newSub  = new List<int>();
            for (int i = 0; i + 2 < origSub.Length; i += 3)
            {
                int globalTri = globalOffset + i / 3;
                if (!markedSet.Contains(globalTri))
                {
                    newSub.Add(origSub[i]); newSub.Add(origSub[i+1]); newSub.Add(origSub[i+2]);
                }
                else removedCount++;
            }
            newMesh.SetTriangles(newSub.ToArray(), sub);
            globalOffset += origSub.Length / 3;
        }

        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();

        // Save as new asset
        string dir = "Assets/OptimizedMeshes";
        if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets", "OptimizedMeshes");
        string path = AssetDatabase.GenerateUniqueAssetPath(dir + "/" + orig.name + "_cut.asset");
        AssetDatabase.CreateAsset(newMesh, path);
        AssetDatabase.SaveAssets();

        // Assign with undo support
        Undo.RecordObject(_uvTargetSMR, "Supprimer triangles UV");
        _uvTargetSMR.sharedMesh = newMesh;
        EditorUtility.SetDirty(_uvTargetSMR);

        Log(LogLevel.Success, removedCount + " triangle(s) supprimes — mesh sauvegarde : " + path);
        _markedTriangles.Clear();
        GenerateUVPreview();
        EditorUtility.DisplayDialog("Termine", removedCount + " triangle(s) supprimes.\nMesh : " + path, "OK");
    }

    private void ApplyMeshCompression()
    {
        if (_avatar == null) return;
        var lvl = _compressionLvl == 0 ? ModelImporterMeshCompression.Off
                : _compressionLvl == 1 ? ModelImporterMeshCompression.Low
                : _compressionLvl == 2 ? ModelImporterMeshCompression.Medium
                :                        ModelImporterMeshCompression.High;

        int n = ApplyMeshCompressionOnTarget(_avatar, lvl, _blendMode);
        EditorUtility.DisplayDialog("Termine",
            n + " mesh(es) optimises." +
            "\nBlend shapes : " + _blendModes[_blendMode] +
            "\nMesh sauvegardes dans Assets/OptimizedMeshes/", "OK");
    }

    private int ApplyMeshCompressionOnTarget(GameObject target, ModelImporterMeshCompression lvl, int blendMode = 2)
    {
        var smrs = target.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        var mrs  = target.GetComponentsInChildren<MeshRenderer>(true);
        int n = 0;
        string outDir = "Assets/OptimizedMeshes";
        EnsureFolder(outDir);

        try
        {
            int total = smrs.Length + mrs.Length;
            int idx = 0;

            foreach (var smr in smrs)
            {
                EditorUtility.DisplayProgressBar("Optimisation mesh", smr.name, (float)idx++ / total);
                if (smr?.sharedMesh == null) continue;
                var opt = BuildOptimizedMesh(smr.sharedMesh, blendMode, outDir);
                if (opt != null)
                {
                    Undo.RecordObject(smr, "Mesh Compression");
                    smr.sharedMesh = opt;
                    EditorUtility.SetDirty(smr);
                    n++;
                }
                smr.updateWhenOffscreen  = false;
                smr.skinnedMotionVectors = false;
                smr.quality = SkinQuality.Bone2;
            }

            foreach (var mr in mrs)
            {
                EditorUtility.DisplayProgressBar("Optimisation mesh", mr.name, (float)idx++ / total);
                var mf = mr.GetComponent<MeshFilter>();
                if (mf?.sharedMesh == null) continue;
                var opt = BuildOptimizedMesh(mf.sharedMesh, blendMode, outDir);
                if (opt != null)
                {
                    Undo.RecordObject(mf, "Mesh Compression");
                    mf.sharedMesh = opt;
                    EditorUtility.SetDirty(mf);
                    n++;
                }
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows    = false;
            }
        }
        finally { EditorUtility.ClearProgressBar(); }

        AssetDatabase.SaveAssets();
        ApplyImporterSettings(target, lvl);
        Log(LogLevel.Success, n + " mesh(es) optimises — " + lvl + " — " + _blendModes[blendMode]);
        return n;
    }

    private Mesh BuildOptimizedMesh(Mesh orig, int blendMode, string outDir)
    {
        if (orig == null) return null;

        // Nom de base sans suffixe _Opt pour eviter Body_Opt_Opt_Opt...
        string baseName = orig.name.EndsWith("_Opt") ? orig.name.Substring(0, orig.name.Length - 4) : orig.name;
        var m = new Mesh { name = baseName + "_Opt", indexFormat = orig.indexFormat };

        // Copie geometrie de base
        m.vertices    = orig.vertices;
        m.normals     = orig.normals;
        m.tangents    = orig.tangents;
        m.uv          = orig.uv;
        m.colors      = orig.colors;
        m.bindposes   = orig.bindposes;
        m.boneWeights = orig.boneWeights;
        if (orig.uv2 != null && orig.uv2.Length == orig.vertexCount) m.uv2 = orig.uv2;
        // uv3/uv4 ignores : inutiles pour Quest

        // Sous-meshes
        m.subMeshCount = orig.subMeshCount;
        for (int s = 0; s < orig.subMeshCount; s++)
            m.SetTriangles(orig.GetTriangles(s), s);

        // Blend shapes — seulement ceux a garder selon le mode
        int kept = 0, removed = 0;
        var dV = new Vector3[orig.vertexCount];
        var dN = new Vector3[orig.vertexCount];
        var dT = new Vector3[orig.vertexCount];
        for (int i = 0; i < orig.blendShapeCount; i++)
        {
            string bsn = orig.GetBlendShapeName(i);
            if (!ShouldKeepBlendShape(bsn, blendMode)) { removed++; continue; }
            int fc = orig.GetBlendShapeFrameCount(i);
            for (int f = 0; f < fc; f++)
            {
                float w = orig.GetBlendShapeFrameWeight(i, f);
                orig.GetBlendShapeFrameVertices(i, f, dV, dN, dT);
                m.AddBlendShapeFrame(bsn, w, dV, dN, dT);
            }
            kept++;
        }

        m.Optimize();
        m.RecalculateBounds();

        // Chemin deterministe : toujours le meme fichier → mise a jour en place si existant
        string path = outDir + "/" + baseName + "_Opt.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        Mesh result;
        if (existing != null)
        {
            // Ecrase les donnees sans changer le GUID → les references de scene restent valides
            EditorUtility.CopySerialized(m, existing);
            EditorUtility.SetDirty(existing);
            result = existing;
        }
        else
        {
            AssetDatabase.CreateAsset(m, path);
            result = m;
        }

        if (removed > 0)
            Log(LogLevel.Info, baseName + " — " + removed + " blend shape(s) retires, " + kept + " gardes → " + path);
        else
            Log(LogLevel.Info, baseName + " → " + path);

        return result;
    }

    private bool ShouldKeepBlendShape(string name, int blendMode)
    {
        if (blendMode == 0) return true;
        if (blendMode == 3) return false;
        if (blendMode == 1) return IsVisemeOrExpr(name);
        string lo = name.ToLower();
        return _visemeNames.Any(v => lo.Contains(v.ToLower()));
    }

    private void ApplyImporterSettings(GameObject target, ModelImporterMeshCompression lvl)
    {
        var seen = new HashSet<string>();
        var toReimport = new List<ModelImporter>();
        foreach (var smr in target.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr?.sharedMesh == null) continue;
            string p = AssetDatabase.GetAssetPath(smr.sharedMesh);
            if (string.IsNullOrEmpty(p) || !seen.Add(p)) continue;
            var imp = AssetImporter.GetAtPath(p) as ModelImporter;
            if (imp == null) continue;
            try { imp.meshCompression = lvl; imp.isReadable = false; EditorUtility.SetDirty(imp); toReimport.Add(imp); }
            catch { }
        }
        AssetDatabase.StartAssetEditing();
        try { foreach (var imp in toReimport) imp.SaveAndReimport(); }
        finally { AssetDatabase.StopAssetEditing(); }
    }


    // ═════════════════════════════════════════════════════════════
    //  TAB 4 — TEXTURES (vignettes, unused scan, override individual)
    // ═════════════════════════════════════════════════════════════
    private void DrawTextures()
    {
        if (_avatar == null) { InfoBox(T("msg.no.avatar.top")); return; }

        SectionLabel(T("tex.section"));
        BeginCard();
        _texPlatform = EditorGUILayout.Popup(new GUIContent(T("lbl.platform"), "Android Quest / PC Standalone / Les deux"), _texPlatform, _platformLabels);
        _texMaxSizeIdx = EditorGUILayout.Popup(new GUIContent("Max Size", "Resolution max des textures"), _texMaxSizeIdx, _maxSizeLabels);
        _texResizeAlgo = EditorGUILayout.Popup(new GUIContent("Resize Algorithm", "Algorithme de redimensionnement"), _texResizeAlgo, _resizeAlgos);
        _texFormatIdx = EditorGUILayout.Popup(new GUIContent("Format", "Format de compression (ASTC pour Quest, BC7 pour PC qualite max)"), _texFormatIdx, _fmtLabels);
        _texCompIdx = EditorGUILayout.Popup(new GUIContent("Compression", "None (qualite max) a High Quality (balance)"), _texCompIdx, _compLabels);
        EditorGUILayout.Space(4);
        _texGenMipmaps = EditorGUILayout.Toggle(new GUIContent("Generate Mip Maps", "Genere les mipmaps (recommande pour perf)"), _texGenMipmaps);
        _texSRGB = EditorGUILayout.Toggle(new GUIContent("sRGB (Color Texture)", "A cocher pour les textures de couleur, pas les normal maps"), _texSRGB);
        _texUseCrunch = EditorGUILayout.Toggle(new GUIContent("Use Crunch Compression", "Compression additionnelle pour reduire la taille fichier"), _texUseCrunch);
        if (_texUseCrunch)
        {
            EditorGUI.indentLevel++;
            _texCrunchQuality = EditorGUILayout.IntSlider("Compressor Quality", _texCrunchQuality, 0, 100);
            EditorGUI.indentLevel--;
        }
        EndCard();

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();
        if (AccentBtn(new GUIContent(T("tex.scan.btn"),"Liste toutes les textures avec vignettes"), GUILayout.Height(30))) ScanTextures();
        if (SuccessBtn(new GUIContent(T("tex.scan.unused.btn"),"Detecte les textures importees mais non referencees"), GUILayout.Height(30))) ScanUnusedTextures();
        if (AccentBtn(T("tex.apply.all.btn"), GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog(T("dlg.confirm"), "Modifier toutes les textures ?", T("btn.apply"), T("dlg.cancel")))
                ApplyTexturesAll();
        }
        if (UndoBtn(T("btn.undo"), GUILayout.Height(30), GUILayout.Width(80))) Undo.PerformUndo();
        EditorGUILayout.EndHorizontal();

        if (_texScanned && _texList.Count > 0)
        {
            EditorGUILayout.Space(6);
            SectionLabel(T("tex.section") + " (" + _texList.Count + ")");
            BeginCard();
            _texScroll = EditorGUILayout.BeginScrollView(_texScroll, GUILayout.MaxHeight(320));
            for (int ti = 0; ti < _texList.Count; ti++)
            {
                if (_texList[ti].texture == null) continue;
                DrawTextureRow(_texList[ti], ti);
            }
            EditorGUILayout.EndScrollView();
            EndCard();
        }

        if (_unusedTextures.Count > 0)
        {
            EditorGUILayout.Space(6);
            SectionLabel(T("tex.unused.section") + " (" + _unusedTextures.Count + ")");
            BeginCard();
            for (int _ti = 0; _ti < Mathf.Min(_unusedTextures.Count, 20); _ti++)
            {
                var t = _unusedTextures[_ti];
                var _tr = EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
                EditorGUI.DrawRect(_tr, _ti % 2 == 0 ? new Color(0,0,0,0) : new Color(1,1,1,0.04f));
                GUILayout.Label(t != null ? t.name : "null", _styleSmall);
                if (SmallBtn("Ping")) { Selection.activeObject = t; EditorGUIUtility.PingObject(t); }
                EditorGUILayout.EndHorizontal();
            }
            if (_unusedTextures.Count > 20) GUILayout.Label("... et " + (_unusedTextures.Count - 20) + " autres", _styleSmall);

            if (DangerBtn(T("tex.delete.unused.btn"), GUILayout.Height(28)))
            {
                if (EditorUtility.DisplayDialog(T("dlg.confirm"),
                    T("dlg.delete") + " " + _unusedTextures.Count + " textures ?", T("dlg.delete"), T("dlg.cancel")))
                    DeleteUnusedTextures();
            }
            EndCard();
        }
    }

    private void DrawTextureRow(TexEntry te, int idx)
    {
        var rowRect = EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(52));
        if (idx % 2 == 1) EditorGUI.DrawRect(rowRect, new Color(1f,1f,1f,0.03f));

        // Vignette 40x40
        GUILayout.Space(2);
        var thumbRect = GUILayoutUtility.GetRect(40, 40, GUILayout.Width(40), GUILayout.Height(40));
        EditorGUI.DrawRect(thumbRect, COL_BG3);
        if (te.texture != null) GUI.DrawTexture(new Rect(thumbRect.x+2, thumbRect.y+2, 36, 36), te.texture, ScaleMode.ScaleToFit);
        GUILayout.Space(6);

        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

        // Ligne 1 : nom + chip taille
        EditorGUILayout.BeginHorizontal();
        var lnk = new GUIStyle(_styleLink){ fontSize=12, fontStyle=FontStyle.Bold };
        if (GUILayout.Button(te.texture.name, lnk, GUILayout.Height(18), GUILayout.ExpandWidth(true)))
        { Selection.activeObject = te.texture; EditorGUIUtility.PingObject(te.texture); }
        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        int targetSz    = te.overridden ? te.overrideSize : _maxSizeVals[_texMaxSizeIdx];
        bool willResize = te.curSize > targetSz;
        bool alreadyOk  = !te.overridden && te.curSize <= targetSz;
        string szLabel  = alreadyOk ? "✓ " + te.curSize + "px" : te.curSize + " → " + targetSz + "px";
        Color szCol     = alreadyOk ? COL_SUCCESS : willResize ? COL_WARN : COL_TEXT_DIM;
        GUILayout.Label(szLabel,
            new GUIStyle(EditorStyles.miniLabel){ normal={textColor=szCol}, fontStyle=(willResize||alreadyOk)?FontStyle.Bold:FontStyle.Normal },
            GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        // Ligne 2 : controles (plus de checkbox — changer le popup active l'override)
        EditorGUILayout.BeginHorizontal();
        var dim = new GUIStyle(EditorStyles.miniLabel){normal={textColor=COL_TEXT_DIM}};
        var ov  = new GUIStyle(EditorStyles.miniLabel){normal={textColor=COL_ACCENT2}};
        var rst = new GUIStyle(EditorStyles.miniLabel){normal={textColor=COL_TEXT_DIM}, hover={textColor=COL_TEXT}};

        GUILayout.Label("Max", dim, GUILayout.Width(28));
        int sizeIdx = te.overridden ? Array.IndexOf(_maxSizeVals, te.overrideSize) : _texMaxSizeIdx;
        if (sizeIdx < 0) sizeIdx = _texMaxSizeIdx;
        int newSizeIdx = EditorGUILayout.Popup(sizeIdx, _maxSizeLabels, GUILayout.Width(72));
        if (newSizeIdx != sizeIdx) { te.overrideSize = _maxSizeVals[newSizeIdx]; te.overridden = true; }
        if (te.overridden) {
            GUILayout.Label("override", ov, GUILayout.Width(48));
            if (GUILayout.Button("↺", rst, GUILayout.Width(16))) { te.overridden = false; te.overrideSize = _maxSizeVals[_texMaxSizeIdx]; }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        } else GUILayout.Label("global", dim, GUILayout.Width(64));

        GUILayout.Space(8);
        GUILayout.Label("Compr.", dim, GUILayout.Width(42));
        int compIdx = te.overrideCompression ? te.overrideCompIdx : _texCompIdx;
        int newCompIdx = EditorGUILayout.Popup(compIdx, _compLabels, GUILayout.Width(112));
        if (newCompIdx != compIdx) { te.overrideCompIdx = newCompIdx; te.overrideCompression = true; }
        if (te.overrideCompression) {
            GUILayout.Label("override", ov, GUILayout.Width(48));
            if (GUILayout.Button("↺", rst, GUILayout.Width(16))) { te.overrideCompression = false; te.overrideCompIdx = _texCompIdx; }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        } else GUILayout.Label("global", dim, GUILayout.Width(64));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // Ligne 3 : info globale
        string info = _platformLabels[_texPlatform] + " · " + _maxSizeLabels[_texMaxSizeIdx] + "px · " +
            _resizeAlgos[_texResizeAlgo] + " · " + _fmtLabels[_texFormatIdx] + " · " + _compLabels[_texCompIdx];
        if (_texGenMipmaps) info += " · Mips";
        if (_texSRGB)       info += " · sRGB";
        if (_texUseCrunch)  info += " · Crunch";
        GUILayout.Label(info, new GUIStyle(EditorStyles.miniLabel){normal={textColor=new Color(COL_TEXT_DIM.r,COL_TEXT_DIM.g,COL_TEXT_DIM.b,0.55f)}});

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0,1,GUILayout.ExpandWidth(true)), COL_SEP);
    }

    private void ScanTextures()
    {
        _texList.Clear();
        var texs = CollectTextures();
        foreach (var t in texs)
        {
            string path = AssetDatabase.GetAssetPath(t);
            _texList.Add(new TexEntry
            {
                texture = t,
                path = path,
                curSize = Mathf.Max(t.width, t.height),
                targetSize = _maxSizeVals[_texMaxSizeIdx],
                overridden = false,
                overrideSize = _maxSizeVals[_texMaxSizeIdx],
                overrideCompression = false,
                overrideCompIdx = _texCompIdx
            });
        }
        _texScanned = true;
        Log(LogLevel.Info, _texList.Count + " textures scannees");
    }

    private void ScanUnusedTextures()
    {
        var usedPaths = new HashSet<string>();
        foreach (var t in CollectTextures())
            usedPaths.Add(AssetDatabase.GetAssetPath(t));

        _unusedTextures.Clear();
        var allTex = AssetDatabase.FindAssets("t:Texture")
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .Where(p => !usedPaths.Contains(p))
            .Where(p => !p.StartsWith("Packages/"))
            .Select(p => AssetDatabase.LoadAssetAtPath<Texture>(p))
            .Where(t => t != null).ToList();
        _unusedTextures = allTex;
        Log(LogLevel.Info, _unusedTextures.Count + " textures inutilisees trouvees dans le projet");
    }

    private void DeleteUnusedTextures()
    {
        int n = 0;
        foreach (var t in _unusedTextures)
        {
            if (t == null) continue;
            var path = AssetDatabase.GetAssetPath(t);
            if (AssetDatabase.DeleteAsset(path)) n++;
        }
        _unusedTextures.Clear();
        Log(LogLevel.Success, n + " textures inutilisees supprimees");
    }

    private void ApplyTexturesAll()
    {
        if (!_texScanned) ScanTextures();
        int n = 0;
        try
        {
            for (int i = 0; i < _texList.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Textures", _texList[i].texture.name, (float)i / _texList.Count);
                var te = _texList[i];
                int size = te.overridden ? te.overrideSize : _maxSizeVals[_texMaxSizeIdx];
                if (!te.overridden && te.curSize <= size) continue; // déjà sous le max global, on ne touche pas
                if (ApplyTextureSettings(te.texture, size, te)) n++;
            }
        }
        finally { EditorUtility.ClearProgressBar(); }
        Log(LogLevel.Success, n + " textures optimisees");
        EditorUtility.DisplayDialog("Termine", n + " textures reimportees.", "OK");
    }

    private bool ApplyTextureSettings(Texture tex, int maxSize, TexEntry te)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        if (string.IsNullOrEmpty(path)) return false;
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) return false;
        bool ch = false;
        if (imp.maxTextureSize > maxSize) { imp.maxTextureSize = maxSize; ch = true; }
        imp.streamingMipmaps = true;
        imp.mipmapEnabled = _texGenMipmaps;
        imp.sRGBTexture = _texSRGB;
        imp.crunchedCompression = _texUseCrunch;
        if (_texUseCrunch) imp.compressionQuality = _texCrunchQuality;

        int compIdx = te.overrideCompression ? te.overrideCompIdx : _texCompIdx;
        string[] plats = _texPlatform == 0 ? new[]{"Android"} : _texPlatform == 1 ? new[]{"Standalone"} : new[]{"Android","Standalone"};
        foreach (var plat in plats)
        {
            var ps = imp.GetPlatformTextureSettings(plat);
            ps.overridden = true; ps.maxTextureSize = maxSize;
            if (_fmtVals[_texFormatIdx] != TextureImporterFormat.Automatic) ps.format = _fmtVals[_texFormatIdx];
            ps.textureCompression = _compVals[compIdx];
            ps.crunchedCompression = _texUseCrunch;
            if (_texUseCrunch) ps.compressionQuality = _texCrunchQuality;
            imp.SetPlatformTextureSettings(ps); ch = true;
        }
        if (ch) { EditorUtility.SetDirty(imp); imp.SaveAndReimport(); }
        return ch;
    }

    private List<Texture> CollectTextures()
    {
        var res = new HashSet<Texture>();
        if (_avatar == null) return res.ToList();
        foreach (var r in _avatar.GetComponentsInChildren<Renderer>(true))
        {
            if (r == null) continue;
            foreach (var mat in r.sharedMaterials)
            {
                if (mat == null) continue;
                int cnt = ShaderUtil.GetPropertyCount(mat.shader);
                for (int i = 0; i < cnt; i++)
                    if (ShaderUtil.GetPropertyType(mat.shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        var t = mat.GetTexture(ShaderUtil.GetPropertyName(mat.shader, i));
                        if (t != null) res.Add(t);
                    }
            }
        }
        return res.ToList();
    }

    // ═════════════════════════════════════════════════════════════
    //  TAB 5 — OUTILS (presets rapides, backup, shaders)
    // ═════════════════════════════════════════════════════════════
    private void OptToggleRow(ref bool val, string label, string tooltip, string icon, Color iconCol, string iconTooltip, float indent = 0f)
    {
        EditorGUILayout.BeginHorizontal();
        if (indent > 0f) GUILayout.Space(indent);
        val = EditorGUILayout.Toggle(new GUIContent(label, tooltip), val);
        GUILayout.Label(new GUIContent(icon, iconTooltip),
            new GUIStyle(EditorStyles.label){ normal={ textColor=iconCol }, fontSize=13 }, GUILayout.Width(18));
        EditorGUILayout.EndHorizontal();
    }

    // ═════════════════════════════════════════════════════════════
    //  TAB 6 — MATERIAUX
    // ═════════════════════════════════════════════════════════════
    private void DrawMaterials()
    {
        if (_avatar == null) { InfoBox(T("msg.no.avatar.top")); return; }

        SectionLabel(T("mat.section"));

        // ── SCAN ─────────────────────────────────────────────────
        BeginCard();
        EditorGUILayout.BeginHorizontal();
        if (AccentBtn(new GUIContent(T("mat.scan.btn"), "Liste tous les materiaux utilises sur l'avatar"), GUILayout.Height(32), GUILayout.ExpandWidth(true)))
            ScanAllMaterials();
        if (_matScanned && UndoBtn(new GUIContent(T("btn.undo"), "Ctrl+Z"), GUILayout.Height(32), GUILayout.Width(90)))
            Undo.PerformUndo();
        EditorGUILayout.EndHorizontal();
        EndCard();

        if (!_matScanned) return;

        // ── STATS ─────────────────────────────────────────────────
        EditorGUILayout.Space(6);
        SectionLabel(T("lbl.stats"));
        int totalSlots   = _matScanList.Sum(e => e.slotCount);
        int uniqueShaders = _matScanList.Select(e => e.shaderName).Distinct().Count();
        int nonQuestMats = _matScanList.Count(e => !e.shaderName.StartsWith("VRChat/Mobile/") && !e.shaderName.Contains("Mobile"));
        BeginCard();
        EditorGUILayout.BeginHorizontal();
        MiniStatTile(T("mat.unique"), _matScanList.Count.ToString(),
            _matScanList.Count > 32 ? COL_ERROR : _matScanList.Count > 16 ? COL_WARN : COL_SUCCESS);
        MiniStatTile(T("mat.slots"), totalSlots.ToString(), COL_ACCENT2);
        MiniStatTile(T("mat.shaders"), uniqueShaders.ToString(), COL_TEXT_DIM);
        MiniStatTile(T("mat.nonquest"), nonQuestMats.ToString(), nonQuestMats > 0 ? COL_WARN : COL_SUCCESS);
        EditorGUILayout.EndHorizontal();
        EndCard();

        // ── LISTE DES MATERIAUX ───────────────────────────────────
        EditorGUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();
        SectionLabel(T("mat.list.section") + " (" + _matScanList.Count + ")");
        GUILayout.FlexibleSpace();
        if (SmallBtn(_matListExpanded ? "▲" : "▼")) _matListExpanded = !_matListExpanded;
        EditorGUILayout.EndHorizontal();

        if (_matListExpanded)
        {
            BeginCard();
            _matListSearch = EditorGUILayout.TextField(GUIContent.none, _matListSearch, EditorStyles.toolbarSearchField);
            EditorGUILayout.Space(4);

            var filtered = string.IsNullOrEmpty(_matListSearch)
                ? _matScanList
                : _matScanList.Where(e => e.mat != null && e.mat.name.ToLowerInvariant().Contains(_matListSearch.ToLowerInvariant())).ToList();

            _matListScroll = EditorGUILayout.BeginScrollView(_matListScroll, GUILayout.MaxHeight(220));
            for (int mi = 0; mi < filtered.Count; mi++)
            {
                var entry = filtered[mi];
                if (entry.mat == null) continue;
                var rowRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(22));
                EditorGUI.DrawRect(rowRect, mi % 2 == 0 ? new Color(0,0,0,0) : new Color(1,1,1,0.03f));

                var icon = AssetPreview.GetMiniTypeThumbnail(typeof(Material));
                GUILayout.Label(new GUIContent(icon), GUILayout.Width(18), GUILayout.Height(18));

                bool isSelected = Selection.activeObject == entry.mat;
                var nameStyle = new GUIStyle(EditorStyles.label) { fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal };
                GUILayout.Label(entry.mat.name, nameStyle, GUILayout.ExpandWidth(true));

                string shaderShort = entry.shaderName.Contains("/")
                    ? entry.shaderName.Substring(entry.shaderName.LastIndexOf('/') + 1)
                    : entry.shaderName;
                GUILayout.Label(shaderShort,
                    new GUIStyle(EditorStyles.miniLabel){ normal={ textColor = COL_TEXT_DIM } },
                    GUILayout.Width(130));
                GUILayout.Label(entry.slotCount + (entry.slotCount > 1 ? " slots" : " slot"),
                    new GUIStyle(EditorStyles.miniLabel){ normal={ textColor = COL_ACCENT2 }, alignment = TextAnchor.MiddleRight },
                    GUILayout.Width(50));

                EditorGUILayout.EndHorizontal();

                if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
                {
                    Selection.activeObject = entry.mat;
                    EditorGUIUtility.PingObject(entry.mat);
                    Event.current.Use();
                }
            }
            EditorGUILayout.EndScrollView();
            EndCard();
        }

        // ── DEDUPLICATION ─────────────────────────────────────────
        EditorGUILayout.Space(6);
        SectionLabel(T("mat.dedup.section"));
        BeginCard();
        GUILayout.Label(T("mat.dedup.info"),
            new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=COL_TEXT_DIM }, wordWrap=true });
        EditorGUILayout.Space(4);
        if (AccentBtn(new GUIContent(T("mat.dedup.btn")), GUILayout.Height(28)))
        {
            int n = RunDedupMaterials();
            ScanAllMaterials();
            if (n > 0) EditorUtility.DisplayDialog("Termine", n + " materiau(x) deduplique(s).", "OK");
            else EditorUtility.DisplayDialog("Info", "Aucun materiau en doublon detecte.", "OK");
        }
        EndCard();

        // ── FIX SHADERS QUEST ─────────────────────────────────────
        EditorGUILayout.Space(6);
        SectionLabel(T("mat.fix.section"));
        BeginCard();
        EditorGUILayout.HelpBox(T("mat.fix.info"), MessageType.Info);
        EditorGUILayout.Space(4);
        _opt_targetShader = EditorGUILayout.Popup(T("mat.fix.shader.target"), _opt_targetShader, _targetShaderNames);
        EditorGUILayout.Space(4);
        if (SmallBtn(T("mat.fix.scan.btn"))) ScanBadShaders();
        if (_matFixScanned)
        {
            EditorGUILayout.Space(4);
            if (_matFixList.Count == 0)
            {
                GUILayout.Label(T("mat.fix.all.ok"), new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=COL_SUCCESS } });
            }
            else
            {
                GUILayout.Label(_matFixList.Count + " materiau(x) avec shaders non-Quest :",
                    new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=COL_WARN }, fontStyle=FontStyle.Bold });
                EditorGUILayout.Space(2);
                _matScroll = EditorGUILayout.BeginScrollView(_matScroll, GUILayout.MaxHeight(200));
                for (int mi = 0; mi < _matFixList.Count; mi++)
                {
                    var m = _matFixList[mi];
                    var rowRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
                    EditorGUI.DrawRect(rowRect, mi % 2 == 0 ? new Color(0,0,0,0) : new Color(1,1,1,0.03f));
                    m.fix = EditorGUILayout.Toggle(m.fix, GUILayout.Width(18));
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(m.mat, typeof(Material), false, GUILayout.ExpandWidth(true));
                    EditorGUI.EndDisabledGroup();
                    GUILayout.Label("→ " + m.currentShader,
                        new GUIStyle(EditorStyles.miniLabel){ normal={ textColor = m.fix ? COL_WARN : COL_TEXT_DIM } },
                        GUILayout.Width(220));
                    EditorGUILayout.EndHorizontal();
                    if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition) && m.mat != null)
                    { EditorGUIUtility.PingObject(m.mat); Event.current.Use(); }
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.Space(6);
                EditorGUI.DrawRect(GUILayoutUtility.GetRect(0,1,GUILayout.ExpandWidth(true)), COL_SEP);
                EditorGUILayout.Space(4);
                GUILayout.Label("OPTIONS TEXTURES (appliquees aux nouveaux materiaux)",
                    new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=COL_ACCENT2 }, fontStyle=FontStyle.Bold });
                EditorGUILayout.Space(2);
                _matTexApply = EditorGUILayout.Toggle(new GUIContent("Appliquer aux textures aussi",
                    "Re-importe les textures des materiaux avec les parametres choisis"), _matTexApply);
                if (_matTexApply)
                {
                    EditorGUI.indentLevel++;
                    _matTexPlatform   = EditorGUILayout.Popup(
                        new GUIContent("Plateforme", "Sur quelle plateforme appliquer la compression"),
                        _matTexPlatform, _platformLabels);
                    _matTexMaxSizeIdx = EditorGUILayout.Popup(
                        new GUIContent("Taille max", "Resolution maximale des textures"),
                        _matTexMaxSizeIdx, _maxSizeLabels);
                    _matTexCompIdx    = EditorGUILayout.Popup(
                        new GUIContent("Compression", "Qualite de compression"),
                        _matTexCompIdx, _compLabels);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space(4);
                int toFix = _matFixList.Count(x => x.fix);
                EditorGUI.BeginDisabledGroup(toFix == 0);
                if (DangerBtn("Appliquer fix sur " + toFix + " materiau(x)", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Confirmation",
                        "Creer " + toFix + " nouveau(x) materiau(x) Quest ?" +
                        (_matTexApply ? "\nTextures : max " + _maxSizeLabels[_matTexMaxSizeIdx] + "px, " + _compLabels[_matTexCompIdx] : "") +
                        "\nCtrl+Z pour annuler.", "Appliquer","Annuler"))
                    {
                        _opt_fixShaders = true;
                        int n = FixShaders();
                        if (_matTexApply) ApplyTexSettingsToFixedMats();
                        ScanAllMaterials();
                        ScanBadShaders();
                        if (n > 0) EditorUtility.DisplayDialog("Termine", n + " materiau(x) remplaces.", "OK");
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
        }
        EndCard();

    }

    private void ScanAllMaterials()
    {
        _matScanList.Clear();
        if (_avatar == null) return;
        var map = new Dictionary<Material, MatScanEntry>();
        foreach (var r in _avatar.GetComponentsInChildren<Renderer>(true))
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m == null) continue;
                if (!map.ContainsKey(m))
                    map[m] = new MatScanEntry { mat = m, shaderName = m.shader != null ? m.shader.name : "null" };
                map[m].slotCount++;
                if (!map[m].meshes.Contains(r.name)) map[m].meshes.Add(r.name);
            }
        }
        _matScanList = map.Values.OrderByDescending(e => e.slotCount).ToList();
        _matScanned = true;
        Log(LogLevel.Info, _matScanList.Count + " materiaux uniques detectes sur " + _avatar.name);
    }

    private void ApplyTexSettingsToFixedMats()
    {
        if (_avatar == null) return;
        var textures = new HashSet<Texture>();
        foreach (var r in _avatar.GetComponentsInChildren<Renderer>(true))
            foreach (var m in r.sharedMaterials)
                if (m != null)
                    foreach (var propName in m.GetTexturePropertyNames())
                    {
                        var t = m.GetTexture(propName);
                        if (t != null) textures.Add(t);
                    }

        int maxSize = _maxSizeVals[_matTexMaxSizeIdx];
        var comp    = _compVals[_matTexCompIdx];
        string platformStr = _matTexPlatform == 0 ? "Android" : _matTexPlatform == 1 ? "Standalone" : null;
        int applied = 0;

        try
        {
            int i = 0;
            foreach (var tex in textures)
            {
                EditorUtility.DisplayProgressBar("Textures", tex.name, (float)i++ / textures.Count);
                string path = AssetDatabase.GetAssetPath(tex);
                if (string.IsNullOrEmpty(path)) continue;
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null) continue;

                bool changed = false;
                if (platformStr != null)
                {
                    var s = imp.GetPlatformTextureSettings(platformStr);
                    if (!s.overridden || s.maxTextureSize != maxSize || s.textureCompression != comp)
                    {
                        s.overridden = true;
                        s.maxTextureSize = maxSize;
                        s.textureCompression = comp;
                        imp.SetPlatformTextureSettings(s);
                        changed = true;
                    }
                }
                else // Quest + PC
                {
                    foreach (var plt in new[]{ "Android","Standalone" })
                    {
                        var s = imp.GetPlatformTextureSettings(plt);
                        s.overridden = true; s.maxTextureSize = maxSize; s.textureCompression = comp;
                        imp.SetPlatformTextureSettings(s);
                    }
                    changed = true;
                }

                if (changed) { imp.SaveAndReimport(); applied++; }
            }
        }
        finally { EditorUtility.ClearProgressBar(); }

        Log(LogLevel.Success, applied + " texture(s) recompressees : max " + maxSize + "px, " + _compLabels[_matTexCompIdx]);
    }

    private void DrawOutils()
    {
        if (_avatar == null) { InfoBox(T("msg.no.avatar.top")); return; }

        SectionLabel(T("tools.presets.section"));
        BeginCard();
        EditorGUILayout.BeginHorizontal();
        DrawQuickPresetCard("Quest Minimum", "Compatibilite maximale Quest", COL_SUCCESS, 0);
        DrawQuickPresetCard("Quest Good", "Bon equilibre quest", COL_WARN, 1);
        DrawQuickPresetCard("PC Excellent", "Qualite PC maximale", COL_ACCENT2, 2);
        DrawQuickPresetCard("Custom", "Config perso", COL_TEXT_DIM, 3);
        EditorGUILayout.EndHorizontal();
        EndCard();

        EditorGUILayout.Space(6);
        SectionLabel(T("tools.backup.section"));
        BeginCard();
        _opt_backup = EditorGUILayout.Toggle(new GUIContent(T("tools.backup.toggle"), "Duplique l'avatar avant toute operation"), _opt_backup);
        if (_opt_backup)
        {
            EditorGUI.indentLevel++;
            _opt_backupPath = EditorGUILayout.TextField(T("lbl.folder.short"), _opt_backupPath);
            EditorGUI.indentLevel--;
        }
        EndCard();

        EditorGUILayout.Space(6);
        SectionLabel(T("tools.clean.section"));
        BeginCard();
        _opt_missingScripts = EditorGUILayout.Toggle(new GUIContent(T("tools.clean.missing"), "Supprime les references de scripts supprimes"),    _opt_missingScripts);
        OptToggleRow(ref _opt_emptyObjects,   T("tools.clean.empty"),   "Supprime les GameObjects sans composant ni enfant",   "⚠", COL_WARN, "Peut supprimer des pivots ou objets de reference");
        _opt_dupMaterials  = EditorGUILayout.Toggle(new GUIContent(T("tools.clean.dedup"),   "Fusionne les materiaux identiques en un seul"),     _opt_dupMaterials);
        _opt_removeAudio   = EditorGUILayout.Toggle(new GUIContent(T("tools.clean.audio"),   "Retire tous les composants AudioSource de l'avatar"), _opt_removeAudio);
        _opt_removeCameras = EditorGUILayout.Toggle(new GUIContent(T("tools.clean.cameras"), "Retire toutes les cameras attachees a l'avatar"),    _opt_removeCameras);
        EditorGUILayout.Space(4);
        if (SmallBtn(T("tools.clean.now.btn")))
        {
            int count = RunDedupMaterials();
            if (count > 0) Log(LogLevel.Success, count + " materiau(x) optimise(s)");
            else Log(LogLevel.Info, "Aucun materiau a optimiser");
        }
        EndCard();

        EditorGUILayout.Space(6);
        SectionLabel(T("tools.anim.section"));
        BeginCard();
        OptToggleRow(ref _opt_animator, T("tools.anim.opt"), "Active les optimisations de l'Animator Controller", "⚡", COL_ACCENT2, "Recommande pour Quest et PC");
        if (_opt_animator)
        {
            OptToggleRow(ref _opt_unusedParams,   T("tools.anim.params"),    "Retire les parametres jamais utilises", "ℹ", COL_TEXT_DIM, "Sur", 14);
            OptToggleRow(ref _opt_emptyLayers,    T("tools.anim.layers"),    "Retire les layers sans aucun state",   "ℹ", COL_TEXT_DIM, "Sur", 14);
            OptToggleRow(ref _opt_cleanKeyframes, T("tools.anim.keyframes"), "Supprime les keyframes identiques",    "⚠", COL_WARN,     "Verifier avant d'appliquer", 14);
        }
        EndCard();

        EditorGUILayout.Space(6);
        SectionLabel(T("tools.shaders.section"));
        BeginCard();
        OptToggleRow(ref _opt_fixShaders, "Fix shaders Quest", "Remplace les shaders PC par des shaders Quest compatibles", "⚠", COL_WARN, "Cree de nouveaux materiaux — l'apparence visuelle peut changer");
        if (_opt_fixShaders)
        {
            EditorGUI.indentLevel++;
            _opt_targetShader = EditorGUILayout.Popup("Remplacer par", _opt_targetShader, _targetShaderNames);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4);
            if (SmallBtn("Scanner shaders non-Quest")) ScanBadShaders();
            if (_matFixScanned)
            {
                GUILayout.Label(_matFixList.Count + " materiaux incompatibles", _styleSmall);
                for (int _oi = 0; _oi < Mathf.Min(_matFixList.Count, 8); _oi++)
                {
                    var m = _matFixList[_oi];
                    var _or = EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
                    EditorGUI.DrawRect(_or, _oi % 2 == 0 ? new Color(0,0,0,0) : new Color(1,1,1,0.04f));
                    m.fix = EditorGUILayout.Toggle(m.fix, GUILayout.Width(16));
                    GUILayout.Label(m.mat.name + " → " + m.currentShader, _styleSmall);
                    EditorGUILayout.EndHorizontal();
                }
                if (_matFixList.Count > 8) GUILayout.Label("... et " + (_matFixList.Count - 8), _styleSmall);
            }
        }
        EndCard();

        EditorGUILayout.Space(6);
        SectionLabel("📉 LOD GROUP");
        BeginCard();
        OptToggleRow(ref _opt_lodGroup, "Generer LOD Group", "Ajoute un LODGroup automatique sur l'avatar", "⚡", COL_ACCENT2, "Ameliore les performances a distance — recommande pour PC");
        if (_opt_lodGroup)
        {
            EditorGUI.indentLevel++;
            _opt_lodDist1 = EditorGUILayout.Slider("LOD1 a", _opt_lodDist1, 0.1f, 5f);
            _opt_lodDist2 = EditorGUILayout.Slider("LOD2 a", _opt_lodDist2, 0.05f, 2f);
            EditorGUI.indentLevel--;
        }
        EndCard();

        EditorGUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        if (AccentBtn("🚀 Lancer l'optimisation", GUILayout.Height(38), GUILayout.ExpandWidth(true)))
        {
            if (EditorUtility.DisplayDialog("VRC Optimizer", "Lancer l'optimisation ?\nCtrl+Z pour annuler.", "Continuer","Annuler"))
                RunOutils();
        }
        if (UndoBtn("Annuler", GUILayout.Height(38), GUILayout.Width(90))) Undo.PerformUndo();
        EditorGUILayout.EndHorizontal();
    }

    private bool PresetBtn(string label, Color col)
    {
        var s = new GUIStyle(GUI.skin.button)
        {
            normal    = { background = MakeTex(1,1, col), textColor = Color.black },
            hover     = { background = MakeTex(1,1, Color.Lerp(col, Color.white, 0.3f)), textColor = Color.black },
            fontStyle = FontStyle.Bold, fontSize = 11
        };
        return GUILayout.Button(label, s, GUILayout.Height(32), GUILayout.ExpandWidth(true));
    }

    private void DrawQuickPresetCard(string label, string subtitle, Color col, int idx)
    {
        bool active = _selectedQuickPreset == idx;
        Color bg  = active ? new Color(col.r*0.18f, col.g*0.18f, col.b*0.18f) : COL_BG2;
        Color bgH = active ? new Color(col.r*0.26f, col.g*0.26f, col.b*0.26f) : COL_BG3;

        var card = new GUIStyle { normal = { background = MakeTex(1,1, bg) }, margin = new RectOffset(2,2,0,0), padding = new RectOffset(8,8,0,8) };
        EditorGUILayout.BeginVertical(card, GUILayout.Width(170), GUILayout.MinHeight(86));

        var bar = GUILayoutUtility.GetRect(0, 3, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(bar, active ? col : COL_SEP);
        GUILayout.Space(7);

        GUILayout.Label(label, new GUIStyle(EditorStyles.boldLabel)
            { normal = { textColor = active ? col : COL_TEXT }, fontSize = 11, alignment = TextAnchor.UpperCenter });
        GUILayout.Space(2);
        GUILayout.Label(subtitle, new GUIStyle(EditorStyles.miniLabel)
            { normal = { textColor = COL_TEXT_DIM }, alignment = TextAnchor.UpperCenter, wordWrap = true });
        GUILayout.FlexibleSpace();

        Color btnCol = active ? col : COL_ACCENT;
        if (GUILayout.Button(active ? "✓ Actif" : "Appliquer", new GUIStyle(GUI.skin.button)
        {
            normal  = { background = MakeTex(1,1, btnCol),                               textColor = Color.white },
            hover   = { background = MakeTex(1,1, Color.Lerp(btnCol,Color.white,0.15f)), textColor = Color.white },
            fontStyle = FontStyle.Bold, border = new RectOffset(0,0,0,0)
        }, GUILayout.Height(22)))
        {
            _selectedQuickPreset = idx;
            ApplyQuickPreset(idx);
        }
        EditorGUILayout.EndVertical();
    }

    private int _bsSelectedPreset = -1;
    private void DrawBSPresetCard(string label, string subtitle, Color col, int idx, System.Action onApply)
    {
        bool active = _bsSelectedPreset == idx;
        Color bg    = active ? new Color(col.r*0.18f, col.g*0.18f, col.b*0.18f) : COL_BG2;
        Color bgH   = active ? new Color(col.r*0.26f, col.g*0.26f, col.b*0.26f) : COL_BG3;

        var card = new GUIStyle { normal = { background = MakeTex(1,1, bg) }, margin = new RectOffset(2,2,0,0), padding = new RectOffset(8,8,0,8) };
        EditorGUILayout.BeginVertical(card, GUILayout.ExpandWidth(true), GUILayout.MinHeight(86));

        var bar = GUILayoutUtility.GetRect(0, 3, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(bar, active ? col : COL_SEP);
        GUILayout.Space(7);

        GUILayout.Label(label, new GUIStyle(EditorStyles.boldLabel)
            { normal = { textColor = active ? col : COL_TEXT }, fontSize = 11, alignment = TextAnchor.UpperCenter });
        GUILayout.Space(2);
        GUILayout.Label(subtitle, new GUIStyle(EditorStyles.miniLabel)
            { normal = { textColor = COL_TEXT_DIM }, alignment = TextAnchor.UpperCenter, wordWrap = true });
        GUILayout.FlexibleSpace();

        Color btnCol = active ? col : COL_ACCENT;
        if (GUILayout.Button(active ? "✓ Actif" : "Appliquer", new GUIStyle(GUI.skin.button)
        {
            normal  = { background = MakeTex(1,1, btnCol),                              textColor = Color.white },
            hover   = { background = MakeTex(1,1, Color.Lerp(btnCol,Color.white,0.15f)), textColor = Color.white },
            fontStyle = FontStyle.Bold, border = new RectOffset(0,0,0,0)
        }, GUILayout.Height(22)))
        { _bsSelectedPreset = idx; onApply(); }

        EditorGUILayout.EndVertical();
    }

    private void ApplyQuickPreset(int idx)
    {
        switch (idx)
        {
            case 0: // Quest Minimum — compatibilite maximale Quest
                // Textures
                _texMaxSizeIdx = 3; _texCompIdx = 3; _texUseCrunch = true; _texPlatform = 0;
                _texGenMipmaps = true; _texSRGB = true; _texResizeAlgo = 1;
                // Mesh
                _meshQuality = 25; _maxTrisQuest = 20000; _blendMode = 2;
                // Backup
                _opt_backup = true;
                // Nettoyage
                _opt_missingScripts = true; _opt_emptyObjects = true; _opt_dupMaterials = true;
                _opt_removeAudio = true; _opt_removeCameras = true;
                // Animator
                _opt_animator = true; _opt_unusedParams = true; _opt_emptyLayers = true; _opt_cleanKeyframes = true;
                // Shaders
                _opt_fixShaders = true; _opt_targetShader = 0;
                // LOD
                _opt_lodGroup = false;
                break;

            case 1: // Quest Good — bon equilibre quest
                // Textures
                _texMaxSizeIdx = 4; _texCompIdx = 2; _texUseCrunch = true; _texPlatform = 0;
                _texGenMipmaps = true; _texSRGB = true; _texResizeAlgo = 1;
                // Mesh
                _meshQuality = 50; _maxTrisQuest = 32000; _blendMode = 1;
                // Backup
                _opt_backup = true;
                // Nettoyage
                _opt_missingScripts = true; _opt_emptyObjects = false; _opt_dupMaterials = true;
                _opt_removeAudio = true; _opt_removeCameras = true;
                // Animator
                _opt_animator = true; _opt_unusedParams = true; _opt_emptyLayers = true; _opt_cleanKeyframes = false;
                // Shaders
                _opt_fixShaders = true; _opt_targetShader = 0;
                // LOD
                _opt_lodGroup = false;
                break;

            case 2: // PC Excellent — qualite PC maximale
                // Textures
                _texMaxSizeIdx = 6; _texCompIdx = 1; _texUseCrunch = false; _texPlatform = 1;
                _texGenMipmaps = true; _texSRGB = true; _texResizeAlgo = 0;
                // Mesh
                _meshQuality = 75; _maxTrisPC = 70000; _blendMode = 0;
                // Backup
                _opt_backup = true;
                // Nettoyage
                _opt_missingScripts = true; _opt_emptyObjects = false; _opt_dupMaterials = true;
                _opt_removeAudio = false; _opt_removeCameras = false;
                // Animator
                _opt_animator = true; _opt_unusedParams = true; _opt_emptyLayers = false; _opt_cleanKeyframes = false;
                // Shaders
                _opt_fixShaders = false;
                // LOD
                _opt_lodGroup = false;
                break;

            case 3: // Custom — ne change rien
                break;
        }
        Log(LogLevel.Info, "Preset applique : " + new[]{"Quest Minimum","Quest Good","PC Excellent","Custom"}[idx]);
        Repaint();
    }

    private void DoBackup()
    {
        if (!Directory.Exists(_opt_backupPath)) Directory.CreateDirectory(_opt_backupPath);
        string bkpName = _avatar.name + "_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var clone = Instantiate(_avatar);
        clone.name = bkpName;
        clone.SetActive(false);
        string path = _opt_backupPath + "/" + bkpName + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(clone, path);
        DestroyImmediate(clone);
        Log(LogLevel.Success, "Backup cree : " + path);
    }

    private void ScanBadShaders()
    {
        _matFixList.Clear();
        if (_avatar == null) return;
        var mats = new HashSet<Material>();
        foreach (var r in _avatar.GetComponentsInChildren<Renderer>(true))
            foreach (var m in r.sharedMaterials)
                if (m != null) mats.Add(m);
        foreach (var m in mats)
        {
            string sn = m.shader.name;
            if (!sn.StartsWith("VRChat/Mobile/") && !sn.Contains("Mobile"))
                _matFixList.Add(new MatFixEntry { mat = m, currentShader = sn, fix = true });
        }
        _matFixScanned = true;
        Log(LogLevel.Info, _matFixList.Count + " materiaux avec shaders non-Quest detectes");
    }

    private void RunOutils()
    {
        var tgt = GetTarget();
        Undo.RegisterFullObjectHierarchyUndo(tgt, "VRC Optimizer — Outils");
        if (_opt_backup) DoBackup();
        int total = 0;
        if (_opt_missingScripts) total += RunMissingScripts();
        if (_opt_emptyObjects)   total += RunEmptyObjects();
        if (_opt_dupMaterials)   total += RunDedupMaterials();
        if (_opt_removeAudio)
        {
            var arr = tgt.GetComponentsInChildren<AudioSource>(true);
            foreach (var a in arr) Undo.DestroyObjectImmediate(a);
            if (arr.Length > 0) { Log(LogLevel.Success, arr.Length + " AudioSource(s) supprime(s)"); total += arr.Length; }
        }
        if (_opt_removeCameras)
        {
            var arr = tgt.GetComponentsInChildren<Camera>(true);
            foreach (var c in arr) Undo.DestroyObjectImmediate(c);
            if (arr.Length > 0) { Log(LogLevel.Success, arr.Length + " Camera(s) supprimee(s)"); total += arr.Length; }
        }
        if (_opt_fixShaders && _matFixScanned) total += FixShaders();
        if (_opt_animator && _opt_cleanKeyframes) total += CleanKeyframes();
        SaveWorkingCopy();
        Log(LogLevel.Success, "Optimisation terminee : " + total + " modification(s)");
        EditorUtility.DisplayDialog("Termine", total + " modification(s) appliquee(s).\n" + (_dupMode ? "Sauvegardé : " + _workingPrefabPath : ""), "OK");
    }

    private int FixShaders()
    {
        var shader = Shader.Find(_targetShaderNames[_opt_targetShader]);
        if (shader == null) { Log(LogLevel.Error, "Shader introuvable : " + _targetShaderNames[_opt_targetShader]); return 0; }
        int n = 0;
        foreach (var m in _matFixList.Where(x => x.fix && x.mat != null))
        {
            var newMat = new Material(shader);
            newMat.CopyPropertiesFromMaterial(m.mat);
            newMat.name = m.mat.name + " (Quest)";

            string srcPath = AssetDatabase.GetAssetPath(m.mat);
            string dir = string.IsNullOrEmpty(srcPath)
                ? "Assets/Materials"
                : System.IO.Path.GetDirectoryName(srcPath).Replace("\\", "/");
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(dir + "/" + newMat.name + ".mat");
            AssetDatabase.CreateAsset(newMat, newPath);

            var renderers = GetTarget().GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r.sharedMaterials.Contains(m.mat))
                {
                    var mats = r.sharedMaterials.ToList();
                    for (int i = 0; i < mats.Count; i++)
                        if (mats[i] == m.mat) mats[i] = newMat;
                    Undo.RecordObject(r, "Replace material");
                    r.sharedMaterials = mats.ToArray();
                }
            }
            n++;
        }
        AssetDatabase.Refresh();
        Log(LogLevel.Success, n + " materiaux crees et remplaces par " + _targetShaderNames[_opt_targetShader]);
        return n;
    }

    private int CleanKeyframes()
    {
        int n = 0;
        var animators = GetTarget().GetComponentsInChildren<Animator>(true);
        foreach (var a in animators)
        {
            if (a.runtimeAnimatorController == null) continue;
            foreach (var clip in a.runtimeAnimatorController.animationClips)
            {
                var bindings = AnimationUtility.GetCurveBindings(clip);
                foreach (var bind in bindings)
                {
                    var curve = AnimationUtility.GetEditorCurve(clip, bind);
                    if (curve == null) continue;
                    var keys = curve.keys;
                    var newKeys = new List<Keyframe>();
                    for (int i = 0; i < keys.Length; i++)
                    {
                        if (i == 0 || i == keys.Length - 1 || Mathf.Abs(keys[i].value - keys[i-1].value) > 0.0001f)
                            newKeys.Add(keys[i]);
                        else n++;
                    }
                    if (newKeys.Count != keys.Length)
                    {
                        curve.keys = newKeys.ToArray();
                        AnimationUtility.SetEditorCurve(clip, bind, curve);
                    }
                }
            }
        }
        if (n > 0) Log(LogLevel.Success, n + " keyframes redondants supprimes");
        return n;
    }

    private int RunMissingScripts()
    {
        var tgt = GetTarget();
        Undo.RegisterFullObjectHierarchyUndo(tgt, "Supprimer scripts manquants");
        int n = 0;
        foreach (var go in tgt.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject))
            n += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        if (n > 0) Log(LogLevel.Success, n + " script(s) manquant(s) supprime(s)");
        return n;
    }

    private int RunEmptyObjects()
    {
        var tgt = GetTarget();
        var usedTransforms = new HashSet<Transform>();
        foreach (var comp in tgt.GetComponentsInChildren<Component>(true))
        {
            if (comp == null) continue;
            var so = new SerializedObject(comp);
            var prop = so.GetIterator();
            while (prop.NextVisible(true))
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (prop.objectReferenceValue is Transform t && t != null)
                        usedTransforms.Add(t);
                    else if (prop.objectReferenceValue is GameObject go2 && go2 != null)
                        usedTransforms.Add(go2.transform);
                }
            }
        }

        var empties = tgt.GetComponentsInChildren<Transform>(true)
            .Select(t => t.gameObject)
            .Where(go => go != tgt
                      && go.GetComponents<Component>().Length == 1
                      && go.transform.childCount == 0
                      && !usedTransforms.Contains(go.transform))
            .ToList();

        foreach (var go in empties) Undo.DestroyObjectImmediate(go);
        if (empties.Count > 0) Log(LogLevel.Success, empties.Count + " objet(s) vide(s) supprime(s)");
        else Log(LogLevel.Info, "Aucun objet vide supprimable");
        return empties.Count;
    }

    private int RunDedupMaterials()
    {
        var tgt = GetTarget();
        var map = new Dictionary<string, Material>();
        int n = 0;
        foreach (var smr in tgt.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            var mats = smr.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                string key = mats[i].name + mats[i].shader.name;
                if (!map.ContainsKey(key)) map[key] = mats[i];
                else if (mats[i] != map[key]) { mats[i] = map[key]; n++; changed = true; }
            }
            if (changed)
            {
                Undo.RecordObject(smr, "Dedupliquer materiaux");
                smr.sharedMaterials = mats;
            }
        }
        if (n > 0) Log(LogLevel.Success, n + " materiau(x) deduplique(s)");
        return n;
    }

    private int RunOptimizeBlendshapes()
    {
        var tgt = GetTarget();
        int totalRemoved = 0;
        foreach (var smr in tgt.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr.sharedMesh == null || smr.sharedMesh.blendShapeCount == 0) continue;
            var blendNames = new List<string>();
            for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++) blendNames.Add(smr.sharedMesh.GetBlendShapeName(i));
            var keep = new List<string>();
            if (_opt_blendMode == 0) keep = blendNames;
            else if (_opt_blendMode == 1) { keep.AddRange(_visemeNames); keep.AddRange(_exprNames); }
            else if (_opt_blendMode == 2) keep.AddRange(_visemeNames);
            else keep.Clear();
            if (keep.Count == blendNames.Count) continue;
            // Create new mesh
            var newMesh = UnityEngine.Object.Instantiate(smr.sharedMesh);
            newMesh.ClearBlendShapes();
            var verts = smr.sharedMesh.vertices;
            var norms = smr.sharedMesh.normals;
            var uvs = smr.sharedMesh.uv;
            var tris = smr.sharedMesh.triangles;
            var colors = smr.sharedMesh.colors;
            var tangents = smr.sharedMesh.tangents;
            newMesh.vertices = verts;
            newMesh.normals = norms;
            newMesh.uv = uvs;
            newMesh.triangles = tris;
            if (colors.Length > 0) newMesh.colors = colors;
            if (tangents.Length > 0) newMesh.tangents = tangents;
            for (int i = 0; i < blendNames.Count; i++)
            {
                if (keep.Contains(blendNames[i]))
                {
                    var deltaVerts = new Vector3[verts.Length];
                    var deltaNorms = new Vector3[norms.Length];
                    var deltaTangents = new Vector3[tangents.Length];
                    smr.sharedMesh.GetBlendShapeFrameVertices(i, 0, deltaVerts, deltaNorms, deltaTangents);
                    newMesh.AddBlendShapeFrame(blendNames[i], 100f, deltaVerts, deltaNorms, deltaTangents);
                }
            }
            Undo.RegisterFullObjectHierarchyUndo(smr, "Optimize Blendshapes");
            smr.sharedMesh = newMesh;
            totalRemoved += blendNames.Count - keep.Count;
        }
        return totalRemoved;
    }

    // ═════════════════════════════════════════════════════════════
    //  BLENDSHAPES UI
    // ═════════════════════════════════════════════════════════════
    private void DrawBlendshapeTab()
    {
        if (_avatar == null) { InfoBox("Assigne un avatar en haut."); return; }
        SectionLabel("🎭 GESTION DES BLENDSHAPES");
        InfoBox("Vert = poids actif (>0%), gris = poids zero. Decocher = sera supprime a l'application.");
        EditorGUILayout.HelpBox(
            "⚠  ATTENTION — Supprimer des blendshapes est irreversible sur le mesh original.\n" +
            "• Visemes (vrc.v_*) : controlent le lip-sync — les perdre casse la bouche en VRChat.\n" +
            "• Expressions (blink, happy…) : controlent les animations faciales.\n" +
            "Sauvegarde ton projet avant toute application. Ctrl+Z disponible apres.",
            MessageType.Warning);
        EditorGUILayout.Space(4);
        DrawBlendshapeSection();
    }

    private void DrawBlendshapeSection()
    {
        // ── SCAN ─────────────────────────────────────────────────
        BeginCard();
        EditorGUILayout.BeginHorizontal();
        if (AccentBtn(new GUIContent("🔍 Scanner", "Liste tous les blendshapes de l'avatar"), GUILayout.Height(32), GUILayout.ExpandWidth(true)))
            ScanBlendshapes();
        if (_bsScanned && UndoBtn(new GUIContent("↶ Annuler","Ctrl+Z"), GUILayout.Height(32), GUILayout.Width(90)))
            Undo.PerformUndo();
        EditorGUILayout.EndHorizontal();
        EndCard();

        if (!_bsScanned) return;
        if (_bsList.Count == 0) { WarnBox("Aucun blendshape trouve."); return; }

        int totalUsed = _bsList.Count(b => b.weight > 0f);
        int totalZero = _bsList.Count - totalUsed;
        int toRemove  = _bsList.Count(b => !b.keep);
        int toKeep    = _bsList.Count - toRemove;

        // ── AUTO-OPTI ────────────────────────────────────────────
        EditorGUILayout.Space(4);
        SectionLabel("⚡ AUTO-OPTIMISATION");
        BeginCard();
        GUILayout.Label("Applique un preset en un clic — cocher/decocher les blendshapes automatiquement :", _styleSmall);
        EditorGUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();

        DrawBSPresetCard("Quest Minimum",  "Garder visemes seuls",    new Color(0.14f,0.56f,0.95f), 0, () => { _bsList.ForEach(b => b.keep = _visemeNames.Any(v => b.name.ToLower().Contains(v.ToLower()))); Repaint(); });
        DrawBSPresetCard("Quest Optimal",  "Visemes + expressions",   new Color(0.25f,0.72f,0.45f), 1, () => { _bsList.ForEach(b => b.keep = IsVisemeOrExpr(b.name)); Repaint(); });
        DrawBSPresetCard("Nettoyer zeros", "Supprimer poids=0",       new Color(0.80f,0.55f,0.10f), 2, () => { _bsList.ForEach(b => b.keep = b.weight > 0f || IsVisemeOrExpr(b.name)); Repaint(); });
        DrawBSPresetCard("Garder actifs",  "Poids > 0 uniquement",    new Color(0.55f,0.30f,0.85f), 3, () => { _bsList.ForEach(b => b.keep = b.weight > 0f); Repaint(); });

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);

        // Barre de preview
        int previewRemove = _bsList.Count(b => !b.keep);
        float removePct = _bsList.Count > 0 ? (float)previewRemove / _bsList.Count : 0f;
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Apercu : " + (_bsList.Count - previewRemove) + " gardés / " + previewRemove + " supprimés",
            new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=COL_TEXT_DIM } });
        EditorGUILayout.EndHorizontal();
        var barR = GUILayoutUtility.GetRect(0, 6, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(barR, COL_BG3);
        EditorGUI.DrawRect(new Rect(barR.x, barR.y, barR.width * (1f - removePct), barR.height), COL_SUCCESS);
        EditorGUI.DrawRect(new Rect(barR.x + barR.width*(1f-removePct), barR.y, barR.width * removePct, barR.height), COL_ERROR);
        EndCard();

        // ── LISTE + PAR MESH (côte à côte) ───────────────────────
        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

        // ════ GAUCHE : liste style Physics ═══════════════════════
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.47f));

        var visible = _bsList.Where(b =>
            string.IsNullOrEmpty(_bsSearch) ||
            b.name.ToLower().Contains(_bsSearch.ToLower()) ||
            b.smr.name.ToLower().Contains(_bsSearch.ToLower())
        ).ToList();
        int vKept = visible.Count(b => b.keep);

        SectionLabel("🎭 BLENDSHAPES — " + vKept + " / " + visible.Count + " gardes");
        BeginCard();
        EditorGUILayout.BeginHorizontal();
        if (SmallBtn("✅ Tout cocher"))         { _bsList.ForEach(b => b.keep = true);  Repaint(); }
        if (DangerSmallBtn("❌ Tout decocher")) { _bsList.ForEach(b => b.keep = false); Repaint(); }
        if (SmallBtn("Decocher non-viseme"))    { _bsList.Where(b=>!IsVisemeOrExpr(b.name)).ToList().ForEach(b=>b.keep=false); Repaint(); }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Rechercher :", new GUIStyle(EditorStyles.label){ normal={ textColor=COL_TEXT_DIM } }, GUILayout.Width(72));
        _bsSearch = EditorGUILayout.TextField(_bsSearch);
        if (SmallBtn("X")) _bsSearch = "";
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);

        _bsScroll = EditorGUILayout.BeginScrollView(_bsScroll, GUILayout.MaxHeight(Mathf.Min(visible.Count * 26 + 40, 300)));
        string lastSmrName = null;
        int _bsRowIdx = 0;
        foreach (var b in visible)
        {
            if (b.smr.name != lastSmrName)
            {
                lastSmrName = b.smr.name;
                _bsRowIdx = 0;
                var grp = _bsList.Where(x => x.smr.name == b.smr.name).ToList();
                int gk = grp.Count(x => x.keep);
                GUILayout.Label("  " + b.smr.name + "  (" + gk + "/" + grp.Count + ")",
                    new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=COL_ACCENT2 }, fontStyle=FontStyle.Bold });
                EditorGUI.DrawRect(GUILayoutUtility.GetRect(0,1,GUILayout.ExpandWidth(true)), COL_SEP);
            }
            var row = EditorGUILayout.BeginHorizontal(GUILayout.Height(24));
            EditorGUI.DrawRect(row, _bsRowIdx % 2 == 0 ? new Color(0,0,0,0) : new Color(1,1,1,0.04f));
            if (!b.keep) EditorGUI.DrawRect(row, new Color(COL_ERROR.r,COL_ERROR.g,COL_ERROR.b,0.15f));
            _bsRowIdx++;
            bool isVis  = _visemeNames.Any(v => b.name.ToLower().Contains(v.ToLower()));
            bool isExpr = _exprNames.Any(e => b.name.ToLower().Contains(e.ToLower()));
            Color tc = b.keep ? COL_ACCENT2 : new Color(COL_ERROR.r, COL_ERROR.g, COL_ERROR.b, 0.8f);
            var ns = new GUIStyle(_styleLink){ normal={ textColor=tc }, fontSize=11 };
            string badge = isVis ? "👄 " : isExpr ? "😊 " : "";
            string prefix = b.keep ? "✓ " : "✗ ";
            if (GUILayout.Button(new GUIContent(prefix + badge + b.name, b.keep ? "Cliquer pour marquer a supprimer" : "Cliquer pour garder"),
                ns, GUILayout.ExpandWidth(true), GUILayout.Height(22)))
            { b.keep = !b.keep; Repaint(); }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EndCard();
        EditorGUILayout.EndVertical(); // fin gauche

        GUILayout.Space(6);

        // ════ DROITE : PAR MESH ══════════════════════════════════
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        SectionLabel("📊 PAR MESH");
        BeginCard();
        foreach (var smr in _bsList.Select(b=>b.smr).Distinct())
        {
            var group   = _bsList.Where(b => b.smr == smr).ToList();
            int gKeep   = group.Count(b => b.keep);
            int gRemove = group.Count - gKeep;
            int gTotal  = group.Count;
            Color dotCol = gRemove == 0 ? COL_SUCCESS : gRemove < gTotal ? COL_WARN : COL_ERROR;

            if (!_bsMeshFoldout.ContainsKey(smr.name)) _bsMeshFoldout[smr.name] = false;
            bool expanded = _bsMeshFoldout[smr.name];

            EditorGUILayout.BeginHorizontal(GUILayout.Height(24));
            string arrow = expanded ? "▾" : "▸";
            if (GUILayout.Button(arrow, new GUIStyle(EditorStyles.label){ normal={ textColor=COL_TEXT_DIM }, fontSize=12 }, GUILayout.Width(16), GUILayout.Height(22)))
                _bsMeshFoldout[smr.name] = !expanded;
            var dotR = GUILayoutUtility.GetRect(8,8,GUILayout.Width(8),GUILayout.Height(24));
            EditorGUI.DrawRect(new Rect(dotR.x, dotR.y+8, 8, 8), dotCol);
            GUILayout.Space(4);
            var ls = new GUIStyle(EditorStyles.label){ normal={ textColor=COL_TEXT }, hover={ textColor=COL_ACCENT2 }, fontStyle=FontStyle.Bold, fontSize=11 };
            if (GUILayout.Button(new GUIContent(smr.name, "Selectionner dans la scene"), ls, GUILayout.ExpandWidth(true), GUILayout.Height(22)))
            {
                Selection.activeGameObject = smr.gameObject;
                EditorGUIUtility.PingObject(smr.gameObject);
                SceneView.FrameLastActiveSceneView();
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            string countTxt = gRemove > 0 ? gKeep + "/" + gTotal + " (-" + gRemove + ")" : gTotal.ToString();
            GUILayout.Label(countTxt, new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=gRemove==0?COL_TEXT_DIM:COL_ERROR }, alignment=TextAnchor.MiddleRight }, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(28);
            var gr = GUILayoutUtility.GetRect(0, 4, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(gr, COL_BG3);
            float gPct = gTotal > 0 ? (float)gKeep / gTotal : 1f;
            if (gPct > 0f) EditorGUI.DrawRect(new Rect(gr.x, gr.y, gr.width*gPct, gr.height), dotCol);
            GUILayout.Space(86);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);

            if (expanded)
            {
                int _bsInnerIdx = 0;
                foreach (var b in group)
                {
                    var row2 = EditorGUILayout.BeginHorizontal(GUILayout.Height(22));
                    EditorGUI.DrawRect(row2, _bsInnerIdx % 2 == 0 ? new Color(0,0,0,0) : new Color(1,1,1,0.04f));
                    if (!b.keep) EditorGUI.DrawRect(row2, new Color(COL_ERROR.r,COL_ERROR.g,COL_ERROR.b,0.15f));
                    _bsInnerIdx++;
                    GUILayout.Space(20);
                    bool iv = _visemeNames.Any(v => b.name.ToLower().Contains(v.ToLower()));
                    bool ie = _exprNames.Any(e => b.name.ToLower().Contains(e.ToLower()));
                    Color tc2 = b.keep ? COL_ACCENT2 : new Color(COL_ERROR.r,COL_ERROR.g,COL_ERROR.b,0.8f);
                    var ns2 = new GUIStyle(_styleLink){ normal={ textColor=tc2 }, fontSize=10 };
                    string pfx2 = b.keep ? "✓ " : "✗ ";
                    if (GUILayout.Button(new GUIContent(pfx2+(iv?"👄 ":ie?"😊 ":"")+b.name, b.keep?"Cliquer pour supprimer":"Cliquer pour garder"),
                        ns2, GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                    { b.keep = !b.keep; Repaint(); }
                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(0,1,GUILayout.ExpandWidth(true)), COL_SEP);
        }
        EndCard();
        EditorGUILayout.EndVertical(); // fin droite

        EditorGUILayout.EndHorizontal(); // fin côte à côte

        // ── APPLY ─────────────────────────────────────────────────
        EditorGUILayout.Space(8);
        if (toRemove > 0)
        {
            var removedList = _bsList.Where(b => !b.keep).ToList();
            int removedVisemes = removedList.Count(b => _visemeNames.Any(v => b.name.ToLower().Contains(v.ToLower())));
            int removedExprs   = removedList.Count(b => _exprNames.Any(e => b.name.ToLower().Contains(e.ToLower())));
            int removedActive  = removedList.Count(b => b.weight > 0f);

            // Severity warning block
            if (removedVisemes > 0)
            {
                var s = new GUIStyle(EditorStyles.helpBox){ fontSize=11, wordWrap=true };
                EditorGUILayout.BeginVertical(s);
                GUILayout.Label("🚨  CRITIQUE — " + removedVisemes + " viseme(s) marque(s) pour suppression",
                    new GUIStyle(EditorStyles.boldLabel){ normal={ textColor=COL_ERROR }, fontSize=12, wordWrap=true });
                GUILayout.Label(
                    "Les visemes controlent la synchronisation labiale de ton avatar dans VRChat. " +
                    "Les supprimer cassera completement le lip-sync — la bouche ne bougera plus quand tu parles.",
                    new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=new Color(1f,0.7f,0.7f) }, wordWrap=true });
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }
            else if (removedExprs > 0)
            {
                var s = new GUIStyle(EditorStyles.helpBox){ fontSize=11, wordWrap=true };
                EditorGUILayout.BeginVertical(s);
                GUILayout.Label("⚠️  ATTENTION — " + removedExprs + " expression(s) marquee(s) pour suppression",
                    new GUIStyle(EditorStyles.boldLabel){ normal={ textColor=COL_WARN }, fontSize=12, wordWrap=true });
                GUILayout.Label(
                    "Ces blendshapes sont utilises pour les expressions du visage (clignements, sourires…). " +
                    "Les supprimer desactivera les animations d'expression de ton avatar.",
                    new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=new Color(1f,0.9f,0.6f) }, wordWrap=true });
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }
            else if (removedActive > 0)
            {
                EditorGUILayout.HelpBox(
                    "⚠  " + removedActive + " blendshape(s) actif(s) (poids > 0) vont etre supprimes. " +
                    "Verifie qu'ils ne sont pas utilises dans des animations avant de continuer.",
                    MessageType.Warning);
                EditorGUILayout.Space(4);
            }

            BeginCard();
            EditorGUILayout.BeginHorizontal();
            var dotR2 = GUILayoutUtility.GetRect(10,10,GUILayout.Width(10),GUILayout.Height(20));
            EditorGUI.DrawRect(new Rect(dotR2.x,dotR2.y+5,10,10), COL_WARN);
            GUILayout.Label(toRemove + " blendshape(s) seront supprimes des meshes — les meshes seront rebuildes.",
                new GUIStyle(EditorStyles.miniLabel){ normal={ textColor=COL_WARN }, wordWrap=true });
            EditorGUILayout.EndHorizontal();
            EndCard();
            EditorGUILayout.Space(4);
        }
        EditorGUI.BeginDisabledGroup(toRemove == 0);
        if (DangerBtn(toRemove > 0 ? "🗑  Appliquer — supprimer " + toRemove + " blendshape(s)" : "Aucun blendshape a supprimer", GUILayout.Height(36)))
        {
            if (EditorUtility.DisplayDialog("Confirmation",
                "Supprimer " + toRemove + " blendshape(s) des meshes ?\n" +
                toKeep + " blendshapes seront conserves.\n\nCette operation rebuilde les meshes affectes.\nCtrl+Z pour annuler.", "Supprimer","Annuler"))
                ApplyBlendshapeRemovals();
        }
        EditorGUI.EndDisabledGroup();
    }

    private GUIStyle MakePresetStyle(Color col)
    {
        return new GUIStyle(GUI.skin.button)
        {
            normal    = { background=MakeTex(1,1,col),                              textColor=Color.white },
            hover     = { background=MakeTex(1,1,Color.Lerp(col,Color.white,0.2f)), textColor=Color.white },
            active    = { background=MakeTex(1,1,Color.Lerp(col,Color.black,0.2f)), textColor=Color.white },
            fontStyle = FontStyle.Bold, fontSize=10,
            alignment = TextAnchor.MiddleCenter,
            wordWrap  = true,
            padding   = new RectOffset(6,6,4,4)
        };
    }

    private bool IsVisemeOrExpr(string name)
    {
        foreach (var v in _visemeNames) if (name.ToLower().Contains(v.ToLower())) return true;
        foreach (var e in _exprNames)   if (name.ToLower().Contains(e.ToLower()))  return true;
        return false;
    }

    private void ScanBlendshapes()
    {
        _bsList.Clear();
        if (_avatar == null) return;
        foreach (var smr in _avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr.sharedMesh == null) continue;
            for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
            {
                string bsName = smr.sharedMesh.GetBlendShapeName(i);
                float  w      = smr.GetBlendShapeWeight(i);
                _bsList.Add(new BSEntry { smr=smr, name=bsName, index=i, weight=w, keep=true });
            }
        }
        _bsScanned = true;
        Log(LogLevel.Info, _bsList.Count + " blendshapes trouves sur " +
            _bsList.Select(b=>b.smr).Distinct().Count() + " mesh(es)");
    }

    private void ApplyBlendshapeRemovals()
    {
        if (_avatar == null) return;
        var smrsToProcess = _bsList.Where(b => !b.keep).Select(b => b.smr).Distinct().ToList();
        int totalRemoved  = 0;

        foreach (var smr in smrsToProcess)
        {
            if (smr == null || smr.sharedMesh == null) continue;
            var mesh        = smr.sharedMesh;
            var keepNames   = _bsList.Where(b => b.smr == smr && b.keep).Select(b => b.name).ToHashSet();
            var allNames    = new List<string>();
            for (int i = 0; i < mesh.blendShapeCount; i++) allNames.Add(mesh.GetBlendShapeName(i));

            var newMesh     = UnityEngine.Object.Instantiate(mesh);
            newMesh.name    = mesh.name;
            newMesh.ClearBlendShapes();

            var verts    = mesh.vertices;
            var norms    = mesh.normals;
            var tangents = mesh.tangents;

            for (int i = 0; i < allNames.Count; i++)
            {
                if (!keepNames.Contains(allNames[i])) { totalRemoved++; continue; }
                var dv = new Vector3[verts.Length];
                var dn = new Vector3[norms.Length];
                var dt = new Vector3[tangents.Length];
                mesh.GetBlendShapeFrameVertices(i, 0, dv, dn, dt);
                newMesh.AddBlendShapeFrame(allNames[i], 100f, dv, dn, dt);
            }

            string srcPath = AssetDatabase.GetAssetPath(mesh);
            string dir     = string.IsNullOrEmpty(srcPath) ? "Assets/Meshes" :
                             System.IO.Path.GetDirectoryName(srcPath).Replace("\\", "/");
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(dir + "/" + newMesh.name + "_BSOptimized.asset");
            AssetDatabase.CreateAsset(newMesh, newPath);

            Undo.RecordObject(smr, "Apply BlendShape Removals");
            smr.sharedMesh = newMesh;
        }
        AssetDatabase.Refresh();
        Log(LogLevel.Success, totalRemoved + " blendshape(s) supprimes sur " + smrsToProcess.Count + " mesh(es)");
        ScanBlendshapes();
    }

    // ═════════════════════════════════════════════════════════════
    //  TAB 6 — PRESETS
    // ═════════════════════════════════════════════════════════════
    private void DrawPresets()
    {
        SectionLabel("➕ CREER UN PRESET");
        BeginCard();
        _presetName = EditorGUILayout.TextField("Nom", _presetName);
        EditorGUILayout.Space(4);
        GUILayout.Label("Inclure dans ce preset :", _styleSmall);
        _presetIncMesh  = EditorGUILayout.Toggle("Mesh", _presetIncMesh);
        _presetIncTex   = EditorGUILayout.Toggle("Textures", _presetIncTex);
        _presetIncPhys  = EditorGUILayout.Toggle("Physics", _presetIncPhys);
        _presetIncTools = EditorGUILayout.Toggle("Outils", _presetIncTools);
        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        if (AccentBtn("💾 Sauvegarder preset", GUILayout.Height(28))) SaveCurrentAsPreset();
        if (SmallBtn("📥 Import JSON")) ImportPresetJson();
        EditorGUILayout.EndHorizontal();
        EndCard();

        EditorGUILayout.Space(6);
        SectionLabel("💾 PRESETS SAUVEGARDES");
        BeginCard();
        _presetScroll = EditorGUILayout.BeginScrollView(_presetScroll, GUILayout.MaxHeight(400));
        if (_presets.Count == 0) GUILayout.Label("Aucun preset.", _styleSmall);
        else
        {
            foreach (var p in _presets.ToList())
            {
                EditorGUILayout.BeginHorizontal(_styleCard);
                GUILayout.Label(p.name, new GUIStyle(EditorStyles.boldLabel){normal={textColor=COL_TEXT}}, GUILayout.ExpandWidth(true));
                string parts = (p.includeMesh?"M":"") + (p.includeTextures?"T":"") + (p.includePhysics?"P":"") + (p.includeTools?"O":"");
                GUILayout.Label("[" + parts + "]", _styleSmall, GUILayout.Width(50));
                if (SmallBtn("Charger")) LoadPreset(p);
                if (SmallBtn("Export")) ExportPresetJson(p);
                if (DangerSmallBtn("X")) { _presets.Remove(p); SavePresets(); }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
        EndCard();
    }

    private void SaveCurrentAsPreset()
    {
        var p = new OptimizerPreset
        {
            name = _presetName,
            includeMesh = _presetIncMesh, includeTextures = _presetIncTex,
            includePhysics = _presetIncPhys, includeTools = _presetIncTools,
            meshQuality = _meshQuality, maxTrisQuest = _maxTrisQuest, maxTrisPC = _maxTrisPC, blendMode = _blendMode,
            texMaxSizeIdx = _texMaxSizeIdx, texFormatIdx = _texFormatIdx, texCompIdx = _texCompIdx,
            texUseCrunch = _texUseCrunch, texSRGB = _texSRGB, texGenMipmaps = _texGenMipmaps,
            texPlatform = _texPlatform, texCrunchQuality = _texCrunchQuality,
            physKeepTopN = _physKeepTopN,
            optMissingScripts = _opt_missingScripts, optEmptyObjects = _opt_emptyObjects,
            optDupMaterials = _opt_dupMaterials, optRemoveAudio = _opt_removeAudio,
            optRemoveCameras = _opt_removeCameras, optFixShaders = _opt_fixShaders,
            optTargetShader = _opt_targetShader
        };
        _presets.Add(p);
        SavePresets();
        Log(LogLevel.Success, "Preset '" + p.name + "' sauvegarde");
    }

    private void LoadPreset(OptimizerPreset p)
    {
        if (p.includeMesh)
        { _meshQuality = p.meshQuality; _maxTrisQuest = p.maxTrisQuest; _maxTrisPC = p.maxTrisPC; _blendMode = p.blendMode; }
        if (p.includeTextures)
        { _texMaxSizeIdx = p.texMaxSizeIdx; _texFormatIdx = p.texFormatIdx; _texCompIdx = p.texCompIdx;
          _texUseCrunch = p.texUseCrunch; _texSRGB = p.texSRGB; _texGenMipmaps = p.texGenMipmaps;
          _texPlatform = p.texPlatform; _texCrunchQuality = p.texCrunchQuality; }
        if (p.includePhysics) _physKeepTopN = p.physKeepTopN;
        if (p.includeTools)
        { _opt_missingScripts = p.optMissingScripts; _opt_emptyObjects = p.optEmptyObjects;
          _opt_dupMaterials = p.optDupMaterials; _opt_removeAudio = p.optRemoveAudio;
          _opt_removeCameras = p.optRemoveCameras; _opt_fixShaders = p.optFixShaders;
          _opt_targetShader = p.optTargetShader; }
        Log(LogLevel.Success, "Preset '" + p.name + "' charge");
    }

    private void SavePresets()
    {
        try
        {
            if (!Directory.Exists("Assets/Editor")) Directory.CreateDirectory("Assets/Editor");
            var sb = new StringBuilder();
            sb.Append("{\"presets\":[");
            for (int i = 0; i < _presets.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(JsonUtility.ToJson(_presets[i]));
            }
            sb.Append("]}");
            File.WriteAllText(PRESETS_FILE, sb.ToString());
            AssetDatabase.Refresh();
        } catch (Exception e) { Log(LogLevel.Error, "Sauvegarde presets : " + e.Message); }
    }

    private void LoadPresets()
    {
        _presets.Clear();
        // Default presets
        _presets.Add(new OptimizerPreset { name = "Quest Compatible", texMaxSizeIdx = 3, texCompIdx = 3, texUseCrunch = true, texPlatform = 0, meshQuality = 50, optFixShaders = true, optRemoveAudio = true });
        _presets.Add(new OptimizerPreset { name = "PC Optimized", texMaxSizeIdx = 6, texCompIdx = 3, texPlatform = 1, meshQuality = 75, blendMode = 0 });
        _presets.Add(new OptimizerPreset { name = "Minimal", texMaxSizeIdx = 2, texCompIdx = 3, texUseCrunch = true, meshQuality = 10, optRemoveAudio = true, optRemoveCameras = true, optFixShaders = true });

        if (File.Exists(PRESETS_FILE))
        {
            try
            {
                var json = File.ReadAllText(PRESETS_FILE);
                var wrapper = JsonUtility.FromJson<PresetWrapper>(json);
                if (wrapper?.presets != null) _presets.AddRange(wrapper.presets);
            } catch { }
        }
    }

    [Serializable] private class PresetWrapper { public OptimizerPreset[] presets; }

    private void ExportPresetJson(OptimizerPreset p)
    {
        var path = EditorUtility.SaveFilePanel("Export preset", "", p.name + ".json", "json");
        if (string.IsNullOrEmpty(path)) return;
        File.WriteAllText(path, JsonUtility.ToJson(p, true));
        Log(LogLevel.Success, "Preset exporte vers " + path);
    }

    private void ImportPresetJson()
    {
        var path = EditorUtility.OpenFilePanel("Import preset", "", "json");
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            var p = JsonUtility.FromJson<OptimizerPreset>(File.ReadAllText(path));
            _presets.Add(p); SavePresets();
            Log(LogLevel.Success, "Preset '" + p.name + "' importe");
        } catch (Exception e) { Log(LogLevel.Error, "Import : " + e.Message); }
    }

    // ═════════════════════════════════════════════════════════════
    //  TAB 7 — LOG (avec filtres, timestamp, export, clic = selection)
    // ═════════════════════════════════════════════════════════════
    private void DrawLog()
    {
        SectionLabel("📝 JOURNAL DES OPERATIONS");

        BeginCard();
        EditorGUILayout.BeginHorizontal();
        _logShowError   = GUILayout.Toggle(_logShowError,   "Erreurs",  EditorStyles.toolbarButton);
        _logShowWarn    = GUILayout.Toggle(_logShowWarn,    "Warnings", EditorStyles.toolbarButton);
        _logShowInfo    = GUILayout.Toggle(_logShowInfo,    "Info",     EditorStyles.toolbarButton);
        _logShowSuccess = GUILayout.Toggle(_logShowSuccess, "Succes",   EditorStyles.toolbarButton);
        GUILayout.FlexibleSpace();
        if (SmallBtn("📤 Exporter .txt")) ExportLogTxt();
        if (SmallBtn("🗑️ Vider")) _log.Clear();
        EditorGUILayout.EndHorizontal();
        EndCard();

        EditorGUILayout.Space(4);

        if (_log.Count == 0) { InfoBox("Aucune operation effectuee."); return; }
        int _logIdx = 0;
        foreach (var e in Enumerable.Reverse(_log))
        {
            if (e.level == LogLevel.Info    && !_logShowInfo)    continue;
            if (e.level == LogLevel.Success && !_logShowSuccess) continue;
            if (e.level == LogLevel.Warn    && !_logShowWarn)    continue;
            if (e.level == LogLevel.Error   && !_logShowError)   continue;

            Color c = e.level switch
            {
                LogLevel.Error   => COL_ERROR,
                LogLevel.Warn    => COL_WARN,
                LogLevel.Success => COL_SUCCESS,
                _                => COL_ACCENT2
            };

            var rect = EditorGUILayout.BeginHorizontal(_styleCard);
            EditorGUI.DrawRect(rect, _logIdx++ % 2 == 0 ? new Color(0,0,0,0) : new Color(1,1,1,0.04f));
            GUILayout.Label(e.time.ToString("HH:mm:ss"), _styleSmall, GUILayout.Width(70));
            GUILayout.Label("[" + e.level + "]", new GUIStyle(EditorStyles.miniLabel){normal={textColor=c}, fontStyle=FontStyle.Bold}, GUILayout.Width(80));
            var ms = new GUIStyle(_styleLink){ normal = { textColor = COL_TEXT }, fontSize = 11 };
            if (GUILayout.Button(e.message, ms, GUILayout.ExpandWidth(true)))
            {
                if (e.objectId != 0)
                {
                    var obj = EditorUtility.InstanceIDToObject(e.objectId);
                    if (obj != null) { Selection.activeObject = obj; EditorGUIUtility.PingObject(obj); }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void ExportLogTxt()
    {
        var path = EditorUtility.SaveFilePanel("Export log", "", "vrc_optimizer_log.txt", "txt");
        if (string.IsNullOrEmpty(path)) return;
        var sb = new StringBuilder();
        sb.AppendLine("NETRA Avatar Optimizer — Log");
        sb.AppendLine(DateTime.Now.ToString());
        sb.AppendLine();
        foreach (var e in _log)
            sb.AppendLine(e.time.ToString("HH:mm:ss") + " [" + e.level + "] " + e.message);
        File.WriteAllText(path, sb.ToString());
        EditorUtility.DisplayDialog("Export", "Log exporte vers :\n" + path, "OK");
    }

    private void Log(LogLevel lvl, string msg, int objId = 0)
    {
        _log.Add(new LogEntry { time = DateTime.Now, level = lvl, message = msg, objectId = objId });
        if (_log.Count > 500) _log.RemoveAt(0);
    }

    // ═════════════════════════════════════════════════════════════
    //  ANALYSE ENGINE
    // ═════════════════════════════════════════════════════════════
    private void RunAnalysis()
    {
        try
        {
            EditorUtility.DisplayProgressBar("Analysing Avatar", "Scanning components...", 0f);
            var r = new AnalyseResults();
            var all = _avatar.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject).ToList();
            r.totalObjects    = all.Count;
            r.disabledObjects = all.Count(g => !g.activeSelf);
            r.missingScripts  = all.Sum(go => go.GetComponents<Component>().Count(c => c == null));
            r.emptyObjects    = all.Count(go => go.GetComponents<Component>().Length == 1 && go.transform.childCount == 0);

            var smrs = _avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var mrs  = _avatar.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var sm in smrs) if (sm.sharedMesh != null) r.totalPolygons += sm.sharedMesh.triangles.Length / 3;
            foreach (var mr in mrs) { var mf = mr.GetComponent<MeshFilter>(); if (mf?.sharedMesh != null) r.totalPolygons += mf.sharedMesh.triangles.Length / 3; }

            var mats = new HashSet<Material>();
            foreach (var sm in smrs) foreach (var m in sm.sharedMaterials) if (m != null) mats.Add(m);
            foreach (var mr in mrs)  foreach (var m in mr.sharedMaterials) if (m != null) mats.Add(m);
            r.totalMaterials = mats.Count;

            var bset = new HashSet<Transform>();
            foreach (var sm in smrs) if (sm.bones != null) foreach (var b in sm.bones) if (b != null) bset.Add(b);
            r.totalBones = bset.Count;

            foreach (var sm in smrs) if (sm.sharedMesh != null) r.totalBlendShapes += sm.sharedMesh.blendShapeCount;

            foreach (var c in _avatar.GetComponentsInChildren<Component>(true))
            {
                if (c == null) continue;
                if (c.GetType().Name == "VRCPhysBone") r.totalPhysBones++;
                if (c.GetType().Name == "VRCPhysBoneCollider") r.totalPhysBoneColliders++;
            }

            r.lightCount = _avatar.GetComponentsInChildren<Light>(true).Length;
            r.particleCount = _avatar.GetComponentsInChildren<ParticleSystem>(true).Length;
            r.audioSourceCount = _avatar.GetComponentsInChildren<AudioSource>(true).Length;
            r.cameraCount = _avatar.GetComponentsInChildren<Camera>(true).Length;
            r.globalRank = CalcRank(r);
            r.questRank = CalcPlatformRank(r, true);
            r.pcRank = CalcPlatformRank(r, false);

            // Projected rank : suppose qu'on applique tout
            var pr = new AnalyseResults();
            pr.totalPolygons = (int)(r.totalPolygons * 0.7f);
            pr.totalMaterials = Mathf.Min(r.totalMaterials, 16);
            pr.totalPhysBones = Mathf.Min(r.totalPhysBones, LIM_PB);
            pr.totalBones = Mathf.Min(r.totalBones, 150);
            pr.totalBlendShapes = r.totalBlendShapes / 2;
            pr.lightCount = 0; pr.particleCount = 0;
            r.projectedRank = CalcRank(pr);
            r.questProjectedRank = CalcPlatformRank(pr, true);
            r.pcProjectedRank = CalcPlatformRank(pr, false);

            _results = r;
            _hasAnalysed = true;
            Log(LogLevel.Info, "Analyse: " + r.totalPolygons + "tris " + r.totalMaterials + "mats " + r.totalPhysBones + "PB rang " + RANK_NAMES[r.globalRank]);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private int CalcRank(AnalyseResults r)
    {
        int rk = 0;
        if (r.totalPolygons  > 70000) rk = Mathf.Max(rk,4); else if (r.totalPolygons  > 32000) rk = Mathf.Max(rk,2);
        if (r.totalMaterials > 32)    rk = Mathf.Max(rk,4); else if (r.totalMaterials > 16)    rk = Mathf.Max(rk,2);
        if (r.totalPhysBones > 64)    rk = Mathf.Max(rk,4); else if (r.totalPhysBones > 32)    rk = Mathf.Max(rk,2);
        if (r.lightCount     > 0)     rk = Mathf.Max(rk,3);
        if (r.particleCount  > 5)     rk = Mathf.Max(rk,3);
        return rk;
    }

    private int CalcPlatformRank(AnalyseResults r, bool quest)
    {
        int rk = 0;
        if (quest)
        {
            if (r.totalPolygons  > 32000) rk = Mathf.Max(rk,4); else if (r.totalPolygons  > 16000) rk = Mathf.Max(rk,2);
            if (r.totalMaterials > 8)     rk = Mathf.Max(rk,4); else if (r.totalMaterials > 4)     rk = Mathf.Max(rk,2);
            if (r.totalPhysBones > 16)    rk = Mathf.Max(rk,4); else if (r.totalPhysBones > 8)     rk = Mathf.Max(rk,2);
        }
        else
        {
            if (r.totalPolygons  > 70000) rk = Mathf.Max(rk,4); else if (r.totalPolygons  > 32000) rk = Mathf.Max(rk,2);
            if (r.totalMaterials > 32)    rk = Mathf.Max(rk,4); else if (r.totalMaterials > 16)    rk = Mathf.Max(rk,2);
            if (r.totalPhysBones > 64)    rk = Mathf.Max(rk,4); else if (r.totalPhysBones > 32)    rk = Mathf.Max(rk,2);
        }
        if (r.lightCount     > 0)     rk = Mathf.Max(rk,3);
        if (r.particleCount  > 5)     rk = Mathf.Max(rk,3);
        return rk;
    }

    // ═════════════════════════════════════════════════════
    //  UI HELPERS
    // ═════════════════════════════════════════════════════════════
    private void BeginCard() => EditorGUILayout.BeginVertical(_styleCard);
    private void EndCard()   => EditorGUILayout.EndVertical();

    private void DrawSep()
    {
        EditorGUILayout.Space(2);
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false,1), COL_SEP);
        EditorGUILayout.Space(2);
    }

    private void SectionLabel(string t) { EditorGUILayout.Space(4); GUILayout.Label(t, _styleSectionLabel); }

    private void MiniStatTile(string label, string val, Color col)
    {
        EditorGUILayout.BeginVertical(_styleCard, GUILayout.ExpandWidth(true));
        GUILayout.Label(val, new GUIStyle(EditorStyles.boldLabel)
        { fontSize = 14, alignment = TextAnchor.MiddleCenter, normal = { textColor = col } }, GUILayout.Height(20));
        GUILayout.Label(label, new GUIStyle(EditorStyles.miniLabel)
        { alignment = TextAnchor.MiddleCenter, normal = { textColor = COL_TEXT_DIM } });
        EditorGUILayout.EndVertical();
    }

    private bool AccentBtn(string label, params GUILayoutOption[] opts) => GUILayout.Button(label, _styleAccentBtn, opts);
    private bool AccentBtn(GUIContent c, params GUILayoutOption[] opts) => GUILayout.Button(c, _styleAccentBtn, opts);
    private bool DangerBtn(string label, params GUILayoutOption[] opts) => GUILayout.Button(label, _styleDangerBtn, opts);
    private bool SuccessBtn(string label, params GUILayoutOption[] opts) => GUILayout.Button(label, _styleSuccessBtn, opts);
    private bool SuccessBtn(GUIContent c, params GUILayoutOption[] opts) => GUILayout.Button(c, _styleSuccessBtn, opts);
    private bool UndoBtn(string label, params GUILayoutOption[] opts) => GUILayout.Button(label, _styleUndoBtn, opts);
    private bool UndoBtn(GUIContent c, params GUILayoutOption[] opts) => GUILayout.Button(c, _styleUndoBtn, opts);

    private bool SmallBtn(string label)
    {
        var s = new GUIStyle(EditorStyles.miniButton) { normal = { textColor = COL_TEXT }, hover = { textColor = Color.white } };
        return GUILayout.Button(label, s);
    }

    private bool DangerSmallBtn(string label)
    {
        var s = new GUIStyle(EditorStyles.miniButton)
        { normal = { background = MakeTex(1,1,new Color(0.4f,0.1f,0.1f)), textColor = new Color(1f,0.6f,0.6f) } };
        return GUILayout.Button(label, s);
    }

    private void InfoBox(string msg) => EditorGUILayout.HelpBox(msg, MessageType.Info);
    private void WarnBox(string msg) => EditorGUILayout.HelpBox(msg, MessageType.Warning);
}

#endif
