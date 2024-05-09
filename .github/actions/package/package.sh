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
  prowlarrFolder="$folderName/Prowlarr"
  archiveName="Prowlarr.$BRANCH.$PROWLARR_VERSION.$name"

  if [[ "$name" == 'UI' ]]; then
    continue
  fi
    
  echo "Creating package for $name"

  echo "Copying UI"
  cp -r $uiFolder $prowlarrFolder
  
  echo "Setting permissions"
  find $prowlarrFolder -name "ffprobe" -exec chmod a+x {} \;
  find $prowlarrFolder -name "Prowlarr" -exec chmod a+x {} \;
  find $prowlarrFolder -name "Prowlarr.Update" -exec chmod a+x {} \;
  
  if [[ "$name" == *"osx"* ]]; then
    echo "Creating macOS package"
      
    packageName="$name-app"
    packageFolder="$outputFolder/$packageName"
      
    rm -rf $packageFolder
    mkdir $packageFolder
      
    cp -r distribution/macOS/Prowlarr.app $packageFolder
    mkdir -p $packageFolder/Prowlarr.app/Contents/MacOS
      
    echo "Copying Binaries"
    cp -r $prowlarrFolder/* $packageFolder/Prowlarr.app/Contents/MacOS
      
    echo "Removing Update Folder"
    rm -r $packageFolder/Prowlarr.app/Contents/MacOS/Prowlarr.Update
              
    echo "Packaging macOS app Artifact"
    (cd $packageFolder; zip -rq "../../$artifactsFolder/$archiveName-app.zip" ./Prowlarr.app)
  fi

  echo "Packaging Artifact"
  if [[ "$name" == *"linux"* ]] || [[ "$name" == *"osx"* ]] || [[ "$name" == *"freebsd"* ]]; then
    tar -zcf "./$artifactsFolder/$archiveName.tar.gz" -C $folderName Prowlarr
	fi
    
  if [[ "$name" == *"win"* ]]; then
    if [ "$RUNNER_OS" = "Windows" ]
      then
        (cd $folderName; 7z a -tzip "../../../$artifactsFolder/$archiveName.zip" ./Prowlarr)
      else
      (cd $folderName; zip -rq "../../../$artifactsFolder/$archiveName.zip" ./Prowlarr)
    fi
	fi
done
