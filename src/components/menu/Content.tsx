import * as RadixMenu from '@radix-ui/react-dropdown-menu';
import clsx from 'clsx';
import React from 'react';

export const MenuContent: React.FC<RadixMenu.DropdownMenuContentProps> = (props) => (
  <RadixMenu.Portal>
    <RadixMenu.Content
      {...props}
      className={clsx(
        props.className,
        'min-w-[250px] bg-gray-800 rounded-lg p-2 shadow-xl border border-gray-700',
        'will-change-[opacity,transform]',

        'data-[state=open]:data-[side=top]:animate-slideAndFadeInFromTop',
        'data-[state=open]:data-[side=right]:animate-slideAndFadeInFromRight',
        'data-[state=open]:data-[side=bottom]:animate-slideAndFadeInFromBottom',
        'data-[state=open]:data-[side=left]:animate-slideAndFadeInFromLeft',

        'data-[state=closed]:data-[side=top]:animate-slideAndFadeOutFromTop',
        'data-[state=closed]:data-[side=right]:animate-slideAndFadeOutFromRight',
        'data-[state=closed]:data-[side=bottom]:animate-slideAndFadeOutFromBottom',
        'data-[state=closed]:data-[side=left]:animate-slideAndFadeOutFromLeft',
      )}
    />
  </RadixMenu.Portal>
);
