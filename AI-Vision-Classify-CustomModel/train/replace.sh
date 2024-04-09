storageAcct='ai102cvcustomnewv4' #replace using your storage account name
sed -i "s/<storageAccount>/$storageAcct/g" training-images/training_labels.json

## make it executable
# chmod +x replace.sh
## run the script
# ./replace.sh