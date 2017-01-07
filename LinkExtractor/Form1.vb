Imports System.Net
Imports System.Text.RegularExpressions

Public Class Form1
    Dim linksHT As Hashtable
    Private Sub ButStart_Click(sender As System.Object, e As System.EventArgs) Handles ButStart.Click
        ExtractLinks(txtURL.Text)
    End Sub
    Public Function ExtractLinks(ByVal url As String) As DataTable
        Dim dt As New DataTable
        dt.Columns.Add("LinkText")
        dt.Columns.Add("LinkUrl")

        Dim wc As New WebClient
        Dim html As String = wc.DownloadString(url)

        Dim links As MatchCollection = Regex.Matches(html, "<a.*?href=""(.*?)"".*?>(.*?)</a>")

        For Each match As Match In links
            Dim dr As DataRow = dt.NewRow
            Dim matchUrl As String = match.Groups(1).Value
            'Ignore all anchor links
            If matchUrl.StartsWith("#") Then
                Continue For
            End If
            'Ignore all javascript calls
            If matchUrl.ToLower.StartsWith("javascript:") Then
                Continue For
            End If
            'Ignore all email links
            If matchUrl.ToLower.StartsWith("mailto:") Then
                Continue For
            End If
            'For internal links, build the url mapped to the base address
            If Not matchUrl.StartsWith("http://") And Not matchUrl.StartsWith("https://") Then
                matchUrl = MapUrl(url, matchUrl)
            End If
            'Add the link data to datatable
            dr("LinkUrl") = matchUrl
            dr("LinkText") = match.Groups(2).Value
            dt.Rows.Add(dr)
        Next

        Return dt
    End Function

    Public Function MapUrl(ByVal baseAddress As String, ByVal relativePath As String) As String

        Dim u As New System.Uri(baseAddress)

        If relativePath = "./" Then
            relativePath = "/"
        End If

        If relativePath.StartsWith("/") Then
            Return u.Scheme + Uri.SchemeDelimiter + u.Authority + relativePath
        Else
            Dim pathAndQuery As String = u.AbsolutePath
            ' If the baseAddress contains a file name, like ..../Something.aspx
            ' Trim off the file name
            pathAndQuery = pathAndQuery.Split("?")(0).TrimEnd("/")
            If pathAndQuery.Split("/")(pathAndQuery.Split("/").Count - 1).Contains(".") Then
                pathAndQuery = pathAndQuery.Substring(0, pathAndQuery.LastIndexOf("/"))
            End If
            baseAddress = u.Scheme + Uri.SchemeDelimiter + u.Authority + pathAndQuery

            'If the relativePath contains ../ then
            ' adjust the baseAddress accordingly

            While relativePath.StartsWith("../")
                relativePath = relativePath.Substring(3)
                If baseAddress.LastIndexOf("/") > baseAddress.IndexOf("//" + 2) Then
                    baseAddress = baseAddress.Substring(0, baseAddress.LastIndexOf("/")).TrimEnd("/")
                End If
            End While

            Return baseAddress + "/" + relativePath
        End If

    End Function

End Class
