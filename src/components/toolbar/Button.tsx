import * as RadixToolbar from '@radix-ui/react-toolbar';

export type ToolbarButtonProps = RadixToolbar.ToolbarButtonProps;

export const ToolbarButton: React.FC<ToolbarButtonProps> = ({ className, ...props }) => {
  return <RadixToolbar.Button {...props} />;
};
