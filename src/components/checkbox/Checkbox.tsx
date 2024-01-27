import * as RadixCheckbox from '@radix-ui/react-checkbox';
import * as RadixLabel from '@radix-ui/react-label';
import clsx from 'clsx';
import { forwardRef, useId } from 'react';
import { FaCheck } from 'react-icons/fa';

import { Icon } from '..';

export type CheckboxProps = RadixCheckbox.CheckboxProps;

export const Checkbox = forwardRef<HTMLButtonElement, CheckboxProps>(
  ({ className, children, ...props }, ref) => {
    const id = useId();

    return (
      <div className={clsx(className, 'flex flex-row gap-2')}>
        <RadixCheckbox.Root
          {...props}
          id={id}
          ref={ref}
          className={
            'flex h-6 w-6 items-center justify-center rounded border border-gray-600 bg-gray-700 transition-colors hover:border-obsidian-500 hover:bg-gray-600'
          }
        >
          <RadixCheckbox.Indicator className="min-h-4 min-w-4 block rounded text-obsidian-500">
            <Icon size="sm" icon={FaCheck} />
          </RadixCheckbox.Indicator>
        </RadixCheckbox.Root>
        <RadixLabel.Root className="text-sm text-gray-50" htmlFor={id}>
          {children}
        </RadixLabel.Root>
      </div>
    );
  },
);
