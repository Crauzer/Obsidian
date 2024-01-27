import React from 'react';

import { Toolbar, ToolbarRootProps } from '../../../../../components';
import { ExtractAllButton } from './ExtractAllButton';

type WadTabToolbarProps = { wadId: string } & ToolbarRootProps;

export const WadTabToolbar: React.FC<WadTabToolbarProps> = ({ wadId, ...props }) => {
  return (
    <Toolbar.Root {...props}>
      <Toolbar.Button asChild>
        <ExtractAllButton wadId={wadId} />
      </Toolbar.Button>
    </Toolbar.Root>
  );
};
