import * as RadixToolbar from '@radix-ui/react-toolbar';
import clsx from 'clsx';

export type ToolbarRootProps = RadixToolbar.ToolbarProps;

export const ToolbarRoot: React.FC<ToolbarRootProps> = ({ className, ...props }) => {
  return (
    <RadixToolbar.Root
      {...props}
      className={clsx(className, 'flex w-full flex-row gap-2 bg-gray-800 px-2 py-1')}
    />
  );
};
