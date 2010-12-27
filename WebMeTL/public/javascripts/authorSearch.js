Marketplace.add({
    label:"Author search",
    icon:"authorSearch.jpg",
    add:function(){
        var id = "findAuthorHost"
        $('#'+id).remove()
        var host = $(sprintf("<div id='%s' title='Find author'></div>",id)).dialog()
        host.append($("<input />").bind('textchange',function(){
            Commands.fire("filterDisplayedAuthors",$(this).val())
        }))
    }
})
