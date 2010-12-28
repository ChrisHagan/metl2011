Marketplace.add({
    label:"Author search",
    icon:"authorSearch.jpg",
    add:function(){
        $('#authorFilter').append($("<input />").bind('textchange',function(){
            Commands.fire("filterDisplayedAuthors",$(this).val())
        }))
    }
})
