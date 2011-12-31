<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Installer.aspx.cs" Inherits="Installer" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Uber Media - Installer</title>
    <style>
    body
    {
        background: #360;
        font: normal 12px Verdana;
    }
    .clear
    {
        clear: both;
    }
    .AREA_WRAPPER
    {
        width: 50em;
        margin: auto;
    }
    .AREA_NAV h1
    {
        float: left;
        margin: 0em; padding: 0em;
        color: #FFF;
        font-size: large;
        -webkit-text-shadow: 0px 0px 5px #000; 
        -moz-text-shadow: 0px 0px 5px #000; 
        text-shadow: 0px 0px 5px #000;
    }
    .AREA_NAV .I
    {
        float: right;
        width: 6em; height: 6em;
        border-radius: 5em;
        background: #FFF;
        color: #360;
        line-height: 6em;
        text-align: center;
        font-size: smaller;
        margin: 0em 2em 1em 0em;
        -webkit-box-shadow: 0px 0px 5px #000; 
        -moz-box-shadow: 0px 0px 5px #000; 
        box-shadow: 0px 0px 5px #000;
    }
    .AREA_NAV .S
    {
        background: #CCC !important;
        color: #333;
    }
    #AREA_CONTENT
    {
        background: #FFF;
        border-radius: 0.25em;
        padding: 0.5em;
    }
    table
    {
        width: 100%;
    }
    table th
    {
        color: #333;
        background: #CCC;
        border-radius: 0.25em;
    }
    h2
    {
        padding: 0em; margin: 0em;
        color: #333;
    }
    input[type=submit]
    {
        border: none;
        color: #FFF;
        background: #360;
        padding: 0.5em;
        cursor: pointer;
    }
    input[type=submit]:hover
    {
        background: #CCC;
        color: #333;
    }
    .ERROR
    {
        display: none;
        visibility: hidden;
        background: #FCC;
        border: solid 0.2em #F00;
        padding: 0.5em;
        margin: 0.5em;
    }
    </style>
</head>
<body>
    <div class="AREA_WRAPPER">
        <div class="AREA_NAV">
            <h1>Uber Media - Installer</h1>
            <div class="I" runat="server" id="C3">Finish</div>
            <div class="I" runat="server" id="C2">Install</div>
            <div class="I" runat="server" id="C1">Config</div>
            <div class="clear"></div>
        </div>
        <div runat="server" id="AREA_CONTENT">
            No content set ;_;...
        </div>
    </div>
</body>
</html>
