function REL_createEventTable()
{
	echo("===Generated Event Datablock Tables===");

	$REL_RegenerateTable = false;
	deleteVariables("$EventDBTable*");

	//this function is the equivalence of ServerConnection.getCount()
	%dbCount = getDatablockGroupSize();
	for(%i = 0; %i < %dbCount; %i++)
	{
		%db = getDatablock(%i);

		%dbClass = %db.getClassName();
		//%tableClass (type of class music/sound/vehicle/ projectiledata)
		//%tableName (display name)

		%hasEventType = false;

		//check the if the current DB matches a specific event type (MUSIC, SOUND, VEHICLE)
		if(%db.uiName $= "")
		{
			if (%dbClass $= "AudioProfile" && %db.getDescription().isLooping != 1 && %db.getDescription().is3D)
			{
				//SOUND TYPE
				%tableName = fileName(%db.fileName);
				%tableClass = "Sound";
				%hasEventType = true;
			}
		} else {
			if(%dbClass $= "AudioProfile")
			{
				//MUSIC TYPE
				%tableName = %db.uiName;
				%tableClass = "Music";
				%hasEventType = true;
			} else if (%dbClass $= "WheeledVehicleData" || %dbClass $= "HoverVehicleData" || %dbClass $= "FlyingVehicleData" || (%dbClass $= "PlayerData" && %db.rideAble))
			{
				//VEHICLE TYPE
				%tableClass = "Vehicle";
				%tableName = %db.uiName;
				%hasEventType = true;
			}
		}

		//register a specific table type
		if(%hasEventType)
		{
			//this is the name to id hash table
			if($EventDBTableClassIndex[%tableClass] $= "")
			{
				$EventDBTableClassIndex[%tableClass] = $EventDBTableClassCount + 0;
				$EventDBTableClassCount++;
			}
			%classIndex = $EventDBTableClassIndex[%tableClass];

			%currentIndex = $EventDBTableCount[%classIndex] + 0; //make sure this is a number

			$EventDBTableName[%classIndex, %currentIndex] = %tableName;
			$EventDBTableID[%classIndex, %currentIndex] = %db;

			$EventDBTableCount[%classIndex]++;
		}

		if(%db.uiName $= "")
			continue;

		//register a classname DB type
		//this is the name to id hash table
		if($EventDBTableClassIndex[%dbClass] $= "")
		{
			$EventDBTableClassIndex[%dbClass] = $EventDBTableClassCount + 0;
			$EventDBTableClassCount++;
		}
		%classIndex = $EventDBTableClassIndex[%dbClass];
		
		%currentIndex = $EventDBTableCount[%classIndex] + 0; //make sure this is a number

		$EventDBTableName[%classIndex, %currentIndex] = %db.uiName;
		$EventDBTableID[%classIndex, %currentIndex] = %db;

		$EventDBTableCount[%classIndex]++;
	}
}

function REL_populateType(%gui, %classType)
{
	if(!isObject(%gui))
		return;

	%index = $EventDBTableClassIndex[%classType];

	%count = $EventDBTableCount[%index];
	for(%i = 0; %i < %count; %i++)
		%gui.add($EventDBTableName[%index, %i], $EventDBTableID[%index, %i]);
}


package Client_ReduceEventLag
{
	//this is called when a datablock is updated ex: transmitdatablocks
	function onDataBlockObjectReceived(%index, %total)
	{
		$REL_RegenerateTable = true;
		return parent::onDataBlockObjectReceived(%index, %total);
	}

	//Generate the datablock list before we start downloading scopealways objects
	function clientCmdMissionStartPhase2(%seq)
	{
		REL_createEventTable();
		return parent::clientCmdMissionStartPhase2(%seq);
	}
	
	function disconnectedCleanup(%doReconnect)
	{
		%parent = parent::disconnectedCleanup(%doReconnect);
		deleteVariables("$EventDBTable*");
		return %parent;
	}

	//function created by Badspot, decompiled from the DSOs, then cleaned-up by Electrk (laggy webpage: https://github.com/Electrk/bl-decompiled/blob/master/client/scripts/allClientScripts.cs#L18263)
	//we need to override this function entirely
	function wrenchEventsDlg::createOutputParameters(%this, %box, %outputMenu, %outputClass)
	{
		//make sure the table is always updated
		if($REL_RegenerateTable)
			REL_createEventTable();

		%this.closeColorMenu();

		//delete the old output parameters
		%lastObject = %box.getObject(%box.getCount() - 1);
		while(%lastObject != %outputMenu)
		{
			%lastObject.delete();
			%lastObject = %box.getObject(%box.getCount() - 1);
		}

		if(%outputMenu.getText() $= "")
			return;

		%selected = %outputMenu.getSelected();
		%parList = $OutputEvent_parameterList[%outputClass, %selected];
		%focusControl = 0;
		%parCount = getFieldCount(%parList);

		for(%i = 0; %i < %parCount; %i++)
		{
			%field = getField(%parList, %i);
			%lastObject = %box.getObject(%box.getCount() - 1);

			%x = 2 + getWord(%lastObject.getPosition(), 0) + getWord(%lastObject.getExtent(), 0);
			%y = 0;
			%h = 18;

			%type = getWord(%field, 0);
			switch$(%type)
			{
				case "int":
					%min = mFloor (getWord (%field, 1));
					%max = mFloor (getWord (%field, 2));
					%default = mFloor (getWord (%field, 3));
					%maxChars = 1;

					if ( %min < 0 )
					{
						%testVal = getMax (mAbs (%min) * 10, %max);
					}
					else
					{
						%testVal = getMax (mAbs (%min), %max);
					}

					while ( %testVal >= 10 )
					{
						%maxChars++;
						%testVal /= 10;
					}

					%gui = new GuiTextEditCtrl ();
					%box.add (%gui);

					%w = (%maxChars * 6) + 6;

					%gui.resize (%x, %y, %w, %h);
					%gui.command = "wrenchEventsDlg.VerifyInt(" @ %gui @ "," @ %min @ "," @ %max @ ");";
					%gui.setText (%default);

				case "intList":
					%maxLength = 200;

					%width = mFloor (getWord (%field, 1));

					%gui = new GuiTextEditCtrl ();
					%box.add (%gui);

					%w = %width;

					%gui.resize (%x, %y, %w, %h);
					%gui.maxLength = %maxLength;

				case "float":
					%min = atof (getWord (%field, 1));
					%max = atof (getWord (%field, 2));
					%step = mAbs (getWord (%field, 3));
					%default = atof (getWord (%field, 4));

					if ( %step >= %max - %min )
					{
						%step = (%max - %min) / 10;
					}

					if ( %step <= 0 )
					{
						%step = 0.1;
					}

					%gui = new GuiSliderCtrl ();
					%box.add (%gui);

					%w = 100;
					%h = 36;

					%gui.resize (%x, %y, %w, %h);
					%gui.range = %min SPC %max;
					%gui.setValue (%default);

					%gui.command = " $thisControl.setValue(       mFloor( $thisControl.getValue() * (1 / "
						@ %step @ ") )   * " @ %step @ "   ) ;";

				case "bool":
					%gui = new GuiCheckBoxCtrl ();
					%box.add (%gui);

					%w = %h;

					%gui.resize (%x, %y, %w, %h);
					%gui.setText ("");

				case "string":
					%maxLength = mFloor (getWord (%field, 1));
					%width = mFloor (getWord (%field, 2));

					%gui = new GuiTextEditCtrl ();
					%box.add (%gui);

					%w = %width;

					%gui.resize (%x, %y, %w, %h);
					%gui.maxLength = %maxLength;

				case "datablock": //THIS IS THE ADDON CUSTOM BEHAVIOUR
						%dbClassName = getWord(%field, 1);
						%gui = new GuiPopUpMenuCtrl();
						%box.add(%gui);
						%gui.resize(%x, %y, 100, %h);

						REL_populateType(%gui, %dbClassName);

						%gui.sort();
						%gui.addFront("NONE", -1);

						if (!$WrenchEventLoading)
							%gui.forceOnAction();

				case "vector":
					%tw = 31;

					%gui = new GuiSwatchCtrl ();
					%box.add (%gui);

					%w = ((%tw + 2) * 3) + 2;

					%gui.resize (%x, %y, %w, %h);
					%gui.setColor ("0 0 0 0.75");

					%xTextBox = new GuiTextEditCtrl ();
					%gui.add (%xTextBox);

					%tx = 0 + 2;
					%ty = 0;
					%th = %h;

					%xTextBox.resize (%tx, %ty, %tw, %th);

					%yTextBox = new GuiTextEditCtrl ();
					%gui.add (%yTextBox);

					%tx = ((%tw + 2) * 1) + 2;
					%yTextBox.resize (%tx, %ty, %tw, %th);

					%zTextBox = new GuiTextEditCtrl ();
					%gui.add (%zTextBox);

					%tx = ((%tw + 2) * 2) + 2;
					%zTextBox.resize (%tx, %ty, %tw, %th);

					%gui = %xTextBox;

				case "list":
					%gui = new GuiPopUpMenuCtrl ();
					%box.add (%gui);

					%w = 100;
					%h = 18;

					%gui.resize (%x, %y, %w, %h);
					%itemCount = (getWordCount (%field) - 1) / 2;

					for ( %itr = 0; %itr < %itemCount; %itr++ )
					{
						%idx = (%itr * 2) + 1;
						%name = getWord (%field, %idx);
						%id = getWord (%field, %idx + 1);

						%gui.add (%name, %id);
					}

					%gui.setSelected (false);

					if ( !$WrenchEventLoading )
					{
						%gui.forceOnAction ();
					}

				case "paintColor":
					%gui = new GuiSwatchCtrl ();
					%box.add (%gui);

					%w = 18;
					%h = 18;

					%gui.resize (%x, %y, %w, %h);

					%button = new GuiBitmapButtonCtrl ();
					%gui.add (%button);

					%button.resize (0, 0, %w, %h);
					%button.setBitmap ("base/client/ui/btnColor");
					%button.setText ("");

					%button.command = "WrenchEventsDlg.CreateColorMenu(" @ %gui @ ");";

					wrenchEventsDlg.pickColor (%gui, 0);

				default:
					error ("ERROR: wrenchEventsDlg::createOutputParameters() - unknown type \"" @ %type @ "\"");
			}

			if (!%focusControl)
				%focusControl = %gui;
		}

		if(isObject(%focusControl))
			%focusControl.makeFirstResponder(1);
	}

	//MEGA support (not tested)
	function populateButton(%button, %datablock)
	{
		//if($EventSystem::Version != 2)
			//return parent::populateButton(%button,%dataBlock);
		%button.dataType = %datablock;
		%button.clear();

		REL_populateType(%button, %dbClassName);

		%button.sort();
		%button.addFront("None",-1);
		%button.entryID["None"] = -1;
	}
};
activatePackage(Client_ReduceEventLag);