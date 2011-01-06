Marketplace.add({
    title:'Threaded discussion',
    icon:'threads.jpg',
    add:function(){
        var id = "IDCommentsPostTitle"
        var server = "http://radar.adm.monash.edu:9000"
        var display = function(jid){
            document.idcomments_acct = 'eadc7f2a59159b61fc9dffd09fce23ff';
            document.idcomments_post_id = sprintf("%s/%s",server,jid);
            document.idcomments_post_url = document.idcomments_post_id;
        }
        $('body').append($(sprintf("<div id='%s'></div>",id)))
        $('body').append($("<script type='text/javascript' src='http://www.intensedebate.com/js/genericCommentWrapperV2.js'></script>"))
        Commands.add("conversationJoined",function(conversation){
            display(conversation.jid)
        })
    }
})
