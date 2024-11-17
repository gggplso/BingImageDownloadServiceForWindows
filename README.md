# BingImageDownloadServiceForWindows

## 版权
本程序软件只是本人初入C#的学习之作，本程序完全开源免费，但根据微软的协议，本程序下载的图片仅限于壁纸使用。它们是受版权保护的图像，因此您不应将其用于其他目的，但可以将其用作桌面壁纸。  
本程序采用的是微软官方公开的接口，是通过JSON数据提取下载路径，从Bing和MSN官网下载图片。不排除以后下载会因微软的调整而失效。

### 介绍
本程序软件基于Windows系统的Bing必应每日壁纸自动下载服务。（不支持其他系统）  
包含内容有：  
1、网络测试；  
2、必应API每日壁纸下载；  
3、Windows聚焦图片复制；  
4、Windows聚焦API图片下载；  
4、图片文件按分辨率横纵比来分类归档。  
（不喜欢的每一项可以单独配置关闭不运行）  


### 软件说明
* 通过访问微软官方的API接口地址来获取图片信息，然后下载图片到本地。  
  * 一个是bing.com的每日图片，一天一张图，通过接口访问共可下载历史15张图，每张图分横向电脑壁纸和竖向手机壁纸，共30个文件。  
  * 一个是Windows聚焦图片，每次访问会随机返回一张图，同样每张图分横向电脑壁纸和竖向手机壁纸两个文件，这里可以通过配置循环次数触发多次访问来获取更多图片。  
  * 另外就是Windows系统本身如果配置过聚焦功能，系统会自动下载图片文件，只是它存放在系统目录路径较深，本程序将其复制到你指定的目录中，并将其添加.png的后缀名，便于系统识别作为壁纸使用。  

* 通过记录文件的HASH值来判断是否已存在，避免重复下载。（以前老版本程序是通过文件名来识别是否重复下载，误判高的离谱。）  

* 通过识别文件的分辨率来分类归档图片。因下载的壁纸有横向高清像素的电脑壁纸和竖向普通像素的手机壁纸，所以本程序将按横纵数值来分类归档图片到不同的目录中。  


### 安装教程

1.  带有扳手和螺丝刀图标的`BingImageDownloadSetting.exe` 文件是配置文件，需要自行配置。软件给了初始默认值，自己需要设置一些图片的保存目录等参数。  
![BingImageDownloadSetting.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadSetting_ico.png)  
注意：第一次用本软件前，一定要先打开使用本文件进行参数配置。


2.  带有微软Bing必应图标的`BingImageDownloadForConsoleApplication.exe`文件是Windows控制台应用程序，可以建立快捷方式添加到开机运行项中自动运行下载任务。  
![BingImageDownloadForConsoleApplication.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadForConsoleApplication_ico.png)  


3.  带有黑色齿轮状图标的`BingImageDownloadServiceForWindows.exe`文件是Windows服务，需要通过双击执行`安装Install.bat`文件将其安装到Windows服务中，以便按定时自动多次运行下载服务。  
![BingImageDownloadServiceForWindows.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadServiceForWindows_ico.png)  


4.  上面`2`和`3`可以任选其一使用，效果是一样的，差异在于`2`是单次运行，`3`是可以通过配置文件设置多次运行。  



### 使用说明

1.  配置服务/程序APP的相关设置。  
![BingImageDownloadSetting.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadSetting_ico_list.png)  
配置是通过WindowsForm的界面可视化窗体设置，配置完成后点击“保存”，配置文件会自动生成到`BingImageDownloadSetting.exe`同目录下。  
![BingImageDownloadSetting.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadSetting_MainSetting.png)
![BingImageDownloadSetting.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadSetting_Setting1.png)
![BingImageDownloadSetting.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadSetting_Setting2.png)
![BingImageDownloadSetting.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadSetting_Setting3.png)  
每一个参数都有默认值，可以不修改就直接使用，也可以根据需求进行修改。每一个配置项后面都跟有文字解释说明，如果有不明白的地方也可以联系我，有些选项鼠标移上去后还会有ToolTip的浮动提示，进一步的解释说明了一些注意事项等。如：  
![BingImageDownloadSetting.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadSetting_TooTip1.png)
![BingImageDownloadSetting.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadSetting_TooTip2.png)


2.  命令控制台程序APP运行。  
![BingImageDownloadForConsoleApplication.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadForConsoleApplication_ico_list.png)  
    * 2.1 将程序APP添加到开机启动项中，步骤如下：  
      * 2.1.1 在程序APP上点击右键，选择“发送到”-“桌面快捷方式”，如果没有这个菜单可能是因为Win11(后)将该菜单收缩在“显示更多选项”中，点击即可。  
![BingImageDownloadForConsoleApplication.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadForConsoleApplication_shell.png)  
      * 2.1.2 在Windows“开始”菜单上点击右键，选择“运行”，输入`shell:startup`回车确定，在打开的文件夹窗口中，将刚才生成的快捷方式拖入到该文件夹中。  
![BingImageDownloadForConsoleApplication.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadForConsoleApplication_startup.png) ![BingImageDownloadForConsoleApplication.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadForConsoleApplication_run.png) ![BingImageDownloadForConsoleApplication.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadForConsoleApplication_start.png)  
 这样每天开机后，系统会自动打开程序，程序APP会自动下载图片到本地。
    * 2.2 运行程序APP，程序APP会自动下载图片到本地，具体存放路径可以在`1`配置服务/程序APP的相关设置中指定。  
![BingImageDownloadForConsoleApplication.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadForConsoleApplication_Download.png)


3.  将服务添加到系统服务列表中。  
![BingImageDownloadServiceForWindows.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadServiceForWindows_ico_list.png)  
    * 3.1 安装服务：双击执行`安装Install.bat`文件，将服务安装到系统服务列表中  
![BingImageDownloadServiceForWindows.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadServiceForWindows_Install.png)  
    * 3.2 检查服务是否启动  
      * 3.2.1 在Windows“开始”菜单上点击右键，选择“运行”，输入`services.msc`回车运行，在打开的系统服务列表窗口中，找到必应每日壁纸下载服务，检查是否正常运行  
![BingImageDownloadServiceForWindows.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadForConsoleApplication_startup.png) ![BingImageDownloadServiceForWindows.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadServiceForWindows_services.png) ![BingImageDownloadServiceForWindows.exe](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/BingImageDownloadServiceForWindows_servicelist.png)  
    * 3.3 卸载服务： 双击执行`卸载Uninstall.bat`文件，将服务从系统服务列表中移除   



### 参与贡献

1.  xxxx
2.  xxxx
3.  xxxx


### 特性

1.  xxxx
2.  xxxx
3.  xxxx


### 修改日志  
曾在2023年上传过第一个版本，但只支持单次Bing壁纸下载，现在已改为支持多次下载，支持Windows服务，支持Windows控制台应用程序，支持配置文件，支持聚焦图片下载，支持图片分类归档。  
<details>
    <summary>
        2024-10-06：上传第一个版本  
    </summary>
</details>
<details>
    <summary>
        2024-10-12：添加了Windows服务  
    </summary>
</details>
<details>
    <summary>
        2024-10-21：解决服务获取用户目录为空，导致服务启动失败问题
    </summary>
</details>
<details>
    <summary>
        2024-10-22：通过获取文件哈希值来判断是否重复下载图片，避免重复下载。弃用原来的通过文件名来判断是否重复的粗糙方式  
    </summary>
</details>
<details>
    <summary>
        2024-10-24：添加Windows聚焦API图片下载功能
    </summary>
</details>
<details>
    <summary>
        2024-11-08：新建WinForm窗体做配置参数可视界面
    </summary> 
</details>


## 成果预览
1、本地建几个文件夹分别用于存放不同的图片，参数设置中会用到这几个文件夹，目录路径需指定，一般习惯是放在Windows用户目录下的图片(转移到D盘，不用系统盘，免得系统盘满)：  
![参数配置](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/参数配置效果0.png)


2、将需要分类的图片放入对应文件夹中，如Downloads：  
![参数配置](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/参数配置效果1.png)  
放在该目录中的图片，程序将会自动按分辨率移动到对应的分类文件夹中，处理完成后，若还有遗留的，可删除。（如果你参数配置无误的话，本目录处理过后，会是空文件夹，若有遗留的，都是重复的图片文件）  
本程序会把横向的图片移动到Computer文件夹中，纵向的图片移动到Mobile文件夹中。  

3、电脑壁纸，Computer：  
![参数配置](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/参数配置效果2.png)


4、手机壁纸，Mobile：  
![参数配置](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/参数配置效果3.png)


5、在Windows系统的个性化设置中配置背景图片为电脑壁纸Computer目录后，设置系统定时轮换，就可以欣赏壁纸了。  
![参数配置](https://gitee.com/gggplso/MarkdownPhotos/raw/master/Photos/BingImageDownloadServiceForWindows/README/参数配置效果4.png)