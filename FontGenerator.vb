Imports System.ComponentModel
Imports System.Text.RegularExpressions

''' <summary>
''' Генератор шрифта.
''' </summary>
Public Class FontGenerator
    Implements INotifyPropertyChanged

#Region "CTOR"

    Public Event PixelsChanged()

    Public Sub New()
        UpdatePixels()
    End Sub

    ''' <summary>
    ''' Создаёт заново массив пикселей в соответствии с размерами символа.
    ''' </summary>
    Private Sub UpdatePixels()
        Pixels.Clear()
        For i As Integer = 0 To FontWidth - 1
            Pixels.Add(New Boolean(FontHeight - 1) {})
        Next
    End Sub

#End Region '/CTOR

#Region "PROPS"

    Public Property FontWidth As Integer
        Get
            Return _FontWidth
        End Get
        Set(value As Integer)
            If (_FontWidth <> value) Then
                If (value > 0) AndAlso (value < 30) Then
                    _FontWidth = value
                    UpdatePixels()
                    NotifyPropertyChanged(NameOf(FontWidth))
                    NotifyPropertyChanged(NameOf(StringArray))
                    NotifyPropertyChanged(NameOf(StringArrayHex))
                    NotifyPropertyChanged(NameOf(StringArrayDec))
                    RaiseEvent PixelsChanged()
                End If
            End If
        End Set
    End Property
    Private _FontWidth As Integer = 6

    Public Property FontHeight As Integer
        Get
            Return _FontHeight
        End Get
        Set(value As Integer)
            If (_FontHeight <> value) Then
                If (value > 0) AndAlso (value < 30) Then
                    _FontHeight = value
                    UpdatePixels()
                    NotifyPropertyChanged(NameOf(FontHeight))
                    NotifyPropertyChanged(NameOf(StringArray))
                    NotifyPropertyChanged(NameOf(StringArrayHex))
                    NotifyPropertyChanged(NameOf(StringArrayDec))
                    RaiseEvent PixelsChanged()
                End If
            End If
        End Set
    End Property
    Private _FontHeight As Integer = 8

    Public Property ByColumn As Boolean
        Get
            Return _ByColumn
        End Get
        Set(value As Boolean)
            If (_ByColumn <> value) Then
                _ByColumn = value
                NotifyPropertyChanged(NameOf(ByColumn))
                NotifyPropertyChanged(NameOf(StringArray))
                NotifyPropertyChanged(NameOf(StringArrayHex))
                NotifyPropertyChanged(NameOf(StringArrayDec))
            End If
        End Set
    End Property
    Private _ByColumn As Boolean = True

    Public Property MsbFirst As Boolean
        Get
            Return _MsbFirst
        End Get
        Set(value As Boolean)
            If (_MsbFirst <> value) Then
                _MsbFirst = value
                NotifyPropertyChanged(NameOf(MsbFirst))
                NotifyPropertyChanged(NameOf(StringArray))
                NotifyPropertyChanged(NameOf(StringArrayHex))
                NotifyPropertyChanged(NameOf(StringArrayDec))
            End If
        End Set
    End Property
    Private _MsbFirst As Boolean = True

#End Region '/PROPS

#Region "READ-ONLY PROPS"

    ''' <summary>
    ''' Массив столбцов символа.
    ''' </summary>
    Public ReadOnly Property Pixels As New List(Of Boolean())

    Public ReadOnly Property StringArray As String
        Get
            Dim sb As New Text.StringBuilder()

            If ByColumn Then
                For i As Integer = 0 To FontWidth - 1
                    sb.Append("0b")
                    If MsbFirst Then
                        For j As Integer = FontHeight - 1 To 0 Step -1
                            sb.Append(If(Pixels(i)(j), "1", "0"))
                        Next
                    Else
                        For j As Integer = 0 To FontHeight - 1
                            sb.Append(If(Pixels(i)(j), "1", "0"))
                        Next
                    End If
                    If (i < FontWidth - 1) Then
                        sb.Append(", ")
                    End If
                Next
            Else
                For j As Integer = 0 To FontHeight - 1
                    sb.Append("0b")
                    If MsbFirst Then
                        For i As Integer = FontWidth - 1 To 0 Step -1
                            sb.Append(If(Pixels(i)(j), "1", "0"))
                        Next
                    Else
                        For i As Integer = 0 To FontWidth - 1
                            sb.Append(If(Pixels(i)(j), "1", "0"))
                        Next
                    End If
                    If (j < FontHeight - 1) Then
                        sb.Append(", ")
                    End If
                Next
            End If

            Return sb.ToString()
        End Get
    End Property

    Public ReadOnly Property StringArrayHex As String
        Get
            Dim sb As New Text.StringBuilder()
            Dim bins As String() = StringArray.Split(","c)
            For i As Integer = 0 To bins.Length - 1
                Dim bin As String = bins(i).Trim().Substring(2)
                Dim value As Integer = Convert.ToInt32(bin, 2)
                sb.Append($"0x{value:X2}, ")
            Next
            sb = sb.Remove(sb.Length - 2, 2)
            Return sb.ToString()
        End Get
    End Property

    Public ReadOnly Property StringArrayDec As String
        Get
            Dim sb As New Text.StringBuilder()
            Dim bins As String() = StringArray.Split(","c)
            For i As Integer = 0 To bins.Length - 1
                Dim bin As String = bins(i).Trim().Substring(2)
                Dim value As Integer = Convert.ToInt32(bin, 2)
                sb.Append($"{value}, ")
            Next
            sb = sb.Remove(sb.Length - 2, 2)
            Return sb.ToString()
        End Get
    End Property

#End Region '/READ-ONLY PROPS

#Region "METHODS"

    ''' <summary>
    ''' Сообщить об изменении пикселей.
    ''' </summary>
    Public Sub TalkPixelChanged(sender As Object, e As EventArgs)
        NotifyPropertyChanged(NameOf(StringArray))
        NotifyPropertyChanged(NameOf(StringArrayHex))
        NotifyPropertyChanged(NameOf(StringArrayDec))
    End Sub

    ''' <summary>
    ''' Сбрасывает все пиксели в "0".
    ''' </summary>
    Public Sub Clear()
        For i As Integer = 0 To Pixels.Count - 1
            For j As Integer = 0 To Pixels(i).Count - 1
                Pixels(i)(j) = False
            Next
        Next
        RaiseEvent PixelsChanged()
    End Sub

    ''' <summary>
    ''' Преобразует текстовое представление массива в значения пикселей.
    ''' </summary>
    ''' <param name="base">Основание, по которому идёт преобразование.</param>
    Public Sub ConvertToSymbol(base As Integer, src As String)
        Try
            'Разбираем массив:
            Dim numRegex As New Regex("(?:0b)(?<num>[01]+)|(?:0x)(?<num>[\dA-Fa-f]+)|(?<num>\d+)")
            Dim ints As New List(Of String)
            Dim mc As MatchCollection = numRegex.Matches(src)
            For Each m As Match In mc
                Dim num As Integer = Convert.ToInt32(m.Groups("num").Value, base)
                Dim bin As String = Convert.ToString(num, 2).PadLeft(If(ByColumn, FontHeight, FontWidth), "0"c)
                If MsbFirst Then
                    Dim rev As Char() = bin.ToCharArray()
                    Array.Reverse(rev)
                    bin = New String(rev)
                End If
                ints.Add(bin)
            Next

            'Выставляем значение пикселей:
            For i As Integer = 0 To FontWidth - 1
                For j As Integer = 0 To FontHeight - 1
                    Dim value As String = If(ByColumn, ints(i)(j), ints(j)(i))
                    Pixels(i)(j) = CBool(value)
                Next
            Next

            RaiseEvent PixelsChanged()

        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub

#End Region '/METHODS

#Region "INOTIFY"

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged
    Private Sub NotifyPropertyChanged(ByVal propName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propName))
    End Sub

#End Region '/INOTIFY

End Class
