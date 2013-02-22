using System;
using System.Collections.Generic;
using System.Text;

namespace AionLauncher
{
    static class Config
    {
        public static int PORT = 2106;
        public static string HOST = "vzoneserver.dyndns.org"; //can be DNS or IP.
        public static string WEBSITE = "http://vzoneserver.dyndns.org/aion/";
        public static string NEWSFEEDURL = "http://cmsstatic.aionfreetoplay.com/launcher_en.html";
        public static string PATCH = "http://vzoneserver.dyndns.org/aion/bin32.zip";

        public static string DEFAULTNEWS = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" +
        "<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
        "<head>" +
        "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />" +
        "<title>News</title>" +
        "<style type=\"text/css\">" +
        "   img{" +
        "   width:100%;" +
        "   height:100%" +
        "   border:0;" +
        "   margin:0px;" +
        "   padding:0px;" +
        "   }" +
        "   body" +
        "   {" +
        "       width:100%;" +
        "       height:100%;" +
        "       margin:0px;" +
        "       overflow:hidden;" +
        "   }" +
        "    p" +
        "    {" +
        "        font-family: Verdana, Geneva, sans-serif;" +
        "        color: #FFF; " +
        "    }" +
        "      .top" +
        "      {" +
        "        padding-top: 0px;" +
        "        margin-top: 0px;  " +
        "      }" +
        "</style>" +
        "</head>" +
        "<body>" +
        "<div id=\"news\"></div>" +
        "</body>" +
        "</html>";

    } //end class
} //end namespace
