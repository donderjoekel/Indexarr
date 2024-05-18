#!/bin/bash

outputFolder=_output
artifactsFolder=_artifacts
uiFolder="$outputFolder/UI"
framework="${FRAMEWORK:=net6.0}"

rm -rf $artifactsFolder
mkdir $artifactsFolder

for runtime in _output/*
do
  name="${runtime##*/}"
  folderName="$runtime/$framework"
  indexarrFolder="$folderName/Indexarr"
  archiveName="Indexarr.$BRANCH.$INDEXARR_VERSION.$name"

  if [[ "$name" == 'UI' ]]; then
    continue
  fi
    
  echo "Creating package for $name"

  echo "Copying UI"
  cp -r $uiFolder $indexarrFolder
  
  echo "Setting permissions"
  find $indexarrFolder -name "ffprobe" -exec chmod a+x {} \;
  find $indexarrFolder -name "Indexarr" -exec chmod a+x {} \;
  find $indexarrFolder -name "Indexarr.Update" -exec chmod a+x {} \;
  
  # if [[ "$name" == *"osx"* ]]; then
  #   echo "Creating macOS package"
      
  #   packageName="$name-app"
  #   packageFolder="$outputFolder/$packageName"
      
  #   rm -rf $packageFolder
  #   mkdir $packageFolder
      
  #   cp -r distribution/macOS/Indexarr.app $packageFolder
  #   mkdir -p $packageFolder/Indexarr.app/Contents/MacOS
      
  #   echo "Copying Binaries"
  #   cp -r $indexarrFolder/* $packageFolder/Indexarr.app/Contents/MacOS
      
  #   echo "Removing Update Folder"
  #   rm -r $packageFolder/Indexarr.app/Contents/MacOS/Indexarr.Update
              
  #   echo "Packaging macOS app Artifact"
  #   (cd $packageFolder; zip -rq "../../$artifactsFolder/$archiveName-app.zip" ./Indexarr.app)
  # fi

  echo "Packaging Artifact"
  if [[ "$name" == *"linux"* ]] || [[ "$name" == *"osx"* ]] || [[ "$name" == *"freebsd"* ]]; then
    tar -zcf "./$artifactsFolder/$archiveName.tar.gz" -C $folderName Indexarr
	fi
    
  if [[ "$name" == *"win"* ]]; then
    if [ "$RUNNER_OS" = "Windows" ]
      then
        (cd $folderName; 7z a -tzip "../../../$artifactsFolder/$archiveName.zip" ./Indexarr)
      else
      (cd $folderName; zip -rq "../../../$artifactsFolder/$archiveName.zip" ./Indexarr)
    fi
	fi
done
