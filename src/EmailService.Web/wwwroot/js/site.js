$(function () {
    $('[data-toggle="popover"]').popover();
    $('form').on('submit', function () {
        $('button[type=submit]', this).button('loading');
    });
});

function bindEditor(target, mode) {
    var textarea = $("textarea[name=" + target + "]").hide();
    var editor = ace.edit(target + "_Editor");
    editor.setOptions({ maxLines: Infinity });
    var session = editor.getSession();
    session.setValue(textarea.val());
    session.setMode(mode);
    session.on("change", function () {
        textarea.val(session.getValue());
    });
}