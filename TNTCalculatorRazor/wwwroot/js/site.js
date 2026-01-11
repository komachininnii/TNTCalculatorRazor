// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ヘルプウィンドウを開く
function openHelpWindow(e, url) {
    e.preventDefault(); // 既定遷移（=二重オープン）を止める

    var name = "TNT_Help";
    var features = "width=900,height=800,resizable=yes,scrollbars=yes";

    var w = window.open(url, name, features);

    // ポップアップがブロックされたら、新しいタブで開く（フォールバック）
    if (!w) {
        window.open(url, "_blank", "noopener");
        return;
    }

    try { w.focus(); } catch (e) { }
}

