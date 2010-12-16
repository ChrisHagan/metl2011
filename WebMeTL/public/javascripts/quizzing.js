var Quizzing = (function(){
    var cornerRadius = "30px"
    Commands.add("conversationJoined",function(conversation){
        $('#quizHost').remove()
        var quizHost = $("<div id='quizHost' title='Quizzes'></div>")
        $('body').append($("<link rel='stylesheet' href='/public/stylesheets/quiz.css'></link>"))
        $('body').append(quizHost)
        quizHost.dialog()
        $.getJSON("/application/quizzes?server="+server+"&jid="+conversation.jid,function(quizzes){
            quizHost.html("")
            _.each(quizzes,function(quiz){
                var qTitle = $("<div class='quiz'>"+quiz.title+"</div>").css("margin-top","1em")
                quizHost.append(qTitle)
                var q = $("<div></div>")
                var optionMarkup = {"padding":"1em"}
                qTitle.after(q)
                _.each(quiz.options,function(option){
                    var oButton = $("<span class='optionButton'>"+option.name+"</span>").corner(cornerRadius).css({"background-color":option.color}).css(optionMarkup)
                    var oText = $("<span class='optionText'>"+option.text+"</span>")
                    var o = $("<div class='quizOption'></div>").corner(cornerRadius)
                    o.append(oButton)
                    o.append(oText)
                    o.click(function(){
                        $.get("/feedback/answerQuiz?username=unknown&password=encrypted&quiz="+quiz.id+"&slide="+quiz.jid+"&answer="+option.name, function(data){
                            alert(data)
                        })
                    })
                    q.append(o)
                }) 
                q.hide()
                qTitle.click(function(){
                   q.toggle() 
                })
            })
        })
    })
})()
