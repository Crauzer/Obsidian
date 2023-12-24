import React from 'react';
import { LuFileStack } from 'react-icons/lu';

import { Button, Icon, Toolbar, ToolbarRootProps } from '../../../../../components';
import { ExtractAllButton } from './ExtractAllButton';

type WadTabToolbarProps = { wadId: string } & ToolbarRootProps;

export const WadTabToolbar: React.FC<WadTabToolbarProps> = ({ wadId, ...props }) => {
  return (
    <Toolbar.Root {...props}>
      <Toolbar.Button asChild>
        <ExtractAllButton wadId={wadId} />
      </Toolbar.Button>
      <Toolbar.Button asChild>
        <Button compact variant="ghost">
          <Icon size="md" icon={LuFileStack} />
        </Button>
      </Toolbar.Button>
    </Toolbar.Root>
  );
};
