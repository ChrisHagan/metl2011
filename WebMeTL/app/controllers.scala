package controllers

import play._
import play.mvc._
import play.libs._
import org.apache.commons.io._
import org.apache.commons.compress.archivers._
import org.apache.commons.compress.archivers.zip._
import java.io.{File}
import scala.xml.parsing._
import scala.collection.JavaConversions._
import org.json.simple._
import net.liftweb._
import net.liftweb.json._
import net.liftweb.json.JsonDSL._

object Application extends Controller {
    val width = 200
    val height = 150
    val server = "https://deified.adm.monash.edu.au:1188/Structure"
    val username = "exampleUsername"
    val password = "examplePassword"
    val TEMP_FILE = "all.zip"
    def index = {
        conversations
    }
    def conversations = {
        val zipFuture = WS.url(server+"/all.zip").authenticate(username,password).get
        FileUtils.writeByteArrayToFile(new File(TEMP_FILE),IOUtils.toByteArray(zipFuture.getStream))
        val zipFile = new ZipFile(new File(TEMP_FILE))
        val xs = zipFile.getEntries
                .map(any => any.asInstanceOf[ZipArchiveEntry])
                .filter(zae=>zae.getName.endsWith("details.xml"))
                .map(zae => IOUtils.toString(zipFile.getInputStream(zae)))
                .map(detail => xml.XML.loadString(detail))
                .toList
        val frequencyJson = JSONObject.toJSONString(authorFrequencies(xs))
        val detailedAuthorInformation = authorConversations(xs).toString()
        val authorJson = detailedAuthorInformation
        Template(frequencyJson, detailedAuthorInformation)
    }
    private def authorFrequencies(xs:Seq[xml.NodeSeq]) = {
        val authors = xs.map(x => (x \ "author").text)
        authors.groupBy(identity).mapValues(_.length)
    }
    private def elem(name:String, children:xml.Node*) = xml.Elem(null, name ,xml.Null,xml.TopScope, children:_*); 
    private def node(name:String, parent:xml.NodeSeq) = elem(name, xml.Text((parent \ name).text))
    private def authorConversations(xs:Seq[xml.NodeSeq]) = {
        val authors = authorFrequencies(xs).map(kv=>{ 
            val author = kv._1
            val conversations = xs.filter(x=>(x \ "author").text == kv._1).map(
                node=>{
                    val contentWeight = elem("authorCount", xml.Text(130.toString))
                    val authorCount = elem("authorCount", xml.Text(13.toString))
                    <conversation>
                        {List("title","jid").map(name=>this.node(name, node)) ::: List(contentWeight, authorCount)}
                    </conversation>;
                })
            val cXml = 
                <conversations author={author}>
                    {conversations}
                </conversations>;
            val jXml = json.Xml.toJson(cXml)
            println(cXml, jXml)
            jXml
        }).reduceLeft(_ merge _)
        pretty(JsonAST.render(authors))
    }
    def conversation(jid:Int)={
        val maybeXml = retrieveDetails(jid).map(renderDetails(_))
        maybeXml match{
            case Some(foundXml)=>{
                val resultXml = foundXml
                Template(resultXml)
            }
            case None=> "Conversation could not be found"
        }
    }
    private def retrieveDetails(id:Int):Option[xml.Node]={
        val uri = "%s/%s/details.xml".format(server,id)
        try{
            println(uri)
            Some(xml.XML.loadString(WS.url(uri).authenticate(username,password).get.getString))
        }
        catch{
            case e: Exception => None
        }
    }
    private def renderDetails(detail:xml.Node):xml.Node={
        val slides = (detail \\ "slide").map(slide =>{
            val thumbUri = "http://spacecaps.adm.monash.edu.au:8080/thumb/%s?width=%d&height=%d".format((slide \\ "id").text, width, height)
            <img src={thumbUri}></img>;
        })
        <div>
            <div>{(detail \\ "title").text}</div>
            {slides}
        </div>;
    }
}
