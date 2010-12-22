Marketplace.add({
    label:"Search",
    icon:"search.jpg",
    add:function(){
        Commands.add("searchForConversations",function(){
            var conversations = _.flatten(_.map(_.keys(detailedAuthors.conversationSummaries),function(author){
                var conversations = detailedAuthors.conversationSummaries[author].conversations.listing
                return conversations.map(function(conversation){
                    conversation.author = author
                    return conversation
                })
            }))
            var id = 'conversationTextNavigation'
            $('#'+id).remove()
            var host = $("<div title='Find a conversation' id='"+id+"'></div>").dialog()
            host.append($("<input />").bind('textchange',function(){
                Commands.fire('filterDisplayedConversations',$(this).val())  
            }))
            var results = $('<div></div>')
            host.append(results)
            Commands.add('filterDisplayedConversations',function(searchTerm){
                results.html("")    
                _.each(_.filter(conversations,function(conversation){
                    return conversation.title.indexOf(searchTerm) >= 0
                }), function(foundConversation){
                    results.append($("<div>"+foundConversation.title+"</div>").click(function(){
                        results.find('div').css('background-color','')
                        $(this).css({'background-color':'yellow'})
                        conversationJoined(foundConversation)
                    }))
                })
            })
        })
        Commands.fire('searchForConversations')
    }
})
