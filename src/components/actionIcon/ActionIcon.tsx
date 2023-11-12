import { Button, ButtonProps, Icon } from '..';

export type ActionIconProps = {
  icon: React.ComponentType<
    React.SVGProps<SVGSVGElement> & {
      title?: string | undefined;
    }
  >;
} & ButtonProps<'button'>;

export const ActionIcon: React.FC<ActionIconProps> = ({ size = 'md', variant, icon, ...props }) => {
  return (
    <Button {...props} compact size={size} variant={variant}>
      <Icon size={size} icon={icon} />
    </Button>
  );
};
