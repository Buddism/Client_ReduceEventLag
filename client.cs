function REL_createEventTable()
{
	newChatHud_AddLine("\c6Generated Event Datablock Tables");

	$REL_RegenerateTable = false;
	deleteVariables("$EventDBTable*");

	%dbCount = getDatablockGroupSize();
	for(%i = 0; %i < %dbCount; %i ++)
	{
		%db = getDatablock(%i);

		%class = %db.getClassName();
		//%tableClass (type of class music/sound/vehicle/ projectiledata)
		//%tableName (display name)

		%hasGenericType = false;

		//check the if the current DB matches a generic event type
		if (%dbClass $= "AudioProfile")
		{
			if (%db.uiName !$= "")
			{
				//MUSIC TYPE
				%tableName = %db.uiName;
				%tableClass = "Music";
				%hasGenericType = true;
			} else if (%db.getDescription().isLooping != 1 && %db.getDescription().is3D)
			{
				//SOUND TYPE
				%tableName = fileName(%db.fileName);
				%tableClass = "Sound";
				%hasGenericType = true;
			}
		} else if(%db.uiName !$= "")
		{
			if (%dbClass $= "WheeledVehicleData" || %dbClass $= "HoverVehicleData" || %dbClass $= "FlyingVehicleData" || (%dbClass $= "PlayerData" && %db.rideAble))
			{
				//VEHICLE TYPE
				%tableClass = "Vehicle";
				%tableName = %db.uiName;
				%hasGenericType = true;
			}
		}

		//register a specific table type
		if(%hasGenericType)
		{
			%classIndex = $EventDBTableClassCount + 0; //make sure this is a number\
			//this is the name to id hash table
			$EventDBTableClassIndex[%tableClass] = %classIndex;

			%currentIndex = $EventDBTableCount[%classIndex] + 0; //make sure this is a number

			$EventDBTableName[%classIndex, %currentIndex] = %tableName;
			$EventDBTableID[%classIndex, %currentIndex] = %db;

			$EventDBTableCount[%classIndex]++;
			$EventDBTableClassCount++;
		}

		if(%db.uiName $= "")
			continue;

		//register a classname DB type
		%classIndex = $EventDBTableClassCount + 0; //make sure this is a number\
		//this is the name to id hash table
		$EventDBTableClassIndex[%class] = %classIndex;
		
		%currentIndex = $EventDBTableCount[%classIndex] + 0; //make sure this is a number

		$EventDBTableName[%classIndex, %currentIndex] = %db.uiName;
		$EventDBTableID[%classIndex, %currentIndex] = %db;

		$EventDBTableCount[%classIndex]++;
		$EventDBTableClassCount++;
	}
}

function REL_populateType(%gui, %classType)
{
	if(!isObject(%gui))
		continue;

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
						%w = 100;
						%gui.resize(%x, %y, %w, %h);
						switch$(%dbClassName)
						{
							case "Music":
								REL_populateType(%gui, "Music");

							case "Sound":
								REL_populateType(%gui, "Sound");

							case "Vehicle":
								REL_populateType(%gui, "Vehicle");
									
							default:
								REL_populateType(%gui, %dbClassName);
						}

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
		switch$(%datablock)
		{
			case "Music":
				REL_populateType(%button, "Music");

			case "Sound":
				REL_populateType(%button, "Sound");

			case "Vehicle":
				REL_populateType(%button, "Vehicle");
					
			default:
				REL_populateType(%button, %dbClassName);
		}

		%button.sort();
		%button.addFront("None",-1);
		%button.entryID["None"] = -1;
	}
};
activatePackage(Client_ReduceEventLag);