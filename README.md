# Annotation data updater
![](https://github.com/Skye-Net/Annotation-data-updater/blob/main/Images/YoloExample.gif "Yolo annotation examples")

Annotation data updater is a console/commandline tool made to interact and modify/update an existing YOLO dataset in a fast and easy way.

[This tool requires .NET 5, which can be downloaded from here.](https://dotnet.microsoft.com/download/dotnet/5.0 "Official .NET 5 download landing page")


## Features
1. Removes "invalid" data from a dataset. Invalid data is a piece of annotated data where there is no object present in the image. In YOLO this is represented by an empty *.txt file. This removes the corresponding image and empty text file
2. Removes images with no corresponding text files
3. Removes a particular annotation class from the testdata
4. Removes datafiles that fulfills a LIKE comparator with another string. Useful when accidentally doubling all data from an accidental copy or something similar
5. Rename annotation
6. Removes annotation text files where there is no corresponding image
7. Automatically extract a random validation set
8. Convert all images to png

## Data updating using the interface
![](https://github.com/Skye-Net/Annotation-data-updater/blob/main/Images/UI_Example.png "The user interface")  
Simply run the program and follow the instructions in the console.

## Data updating from the commandline
When using the data updater from the commandline, the following order of arguments are expected.

1. The action number you wish to be perform, see Features for a the list of valid options
2. The path with data
3. Optional parameter for Action 3 (Class to remove), 4 (Like comparator string) and 7 (Optional percent to extract as validation set)

Example:
In this case I've changed directory to the location of the `AnnotateDataUpdate` executable. Thus to perform Action 1 on a particular folder I run:
```
.\AnnotateDataUpdate.exe 1 "F:\NN Stuff\Annotated data"
```
