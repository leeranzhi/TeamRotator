import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  CircularProgress,
  Snackbar,
  Alert,
} from '@mui/material';
import { useQuery, useMutation } from '@tanstack/react-query';
import { getSettings, updateSettings } from '../services/api';

const Settings: React.FC = () => {
  const [showSuccess, setShowSuccess] = useState(false);
  const [webhookUrl, setWebhookUrl] = useState('');

  const { data: settings, isLoading } = useQuery({
    queryKey: ['settings'],
    queryFn: getSettings,
  });

  useEffect(() => {
    if (settings?.slackWebhookUrl) {
      setWebhookUrl(settings.slackWebhookUrl);
    }
  }, [settings]);

  const updateMutation = useMutation({
    mutationFn: updateSettings,
    onSuccess: () => {
      setShowSuccess(true);
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    updateMutation.mutate({ slackWebhookUrl: webhookUrl });
  };

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="50vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Settings
      </Typography>

      <Card>
        <CardContent>
          <form onSubmit={handleSubmit}>
            <TextField
              label="Slack Webhook URL"
              fullWidth
              margin="normal"
              value={webhookUrl}
              onChange={(e) => setWebhookUrl(e.target.value)}
              type="url"
              required
              helperText="Enter the Slack Webhook URL for notifications"
            />
            <Box mt={2}>
              <Button
                type="submit"
                variant="contained"
                color="primary"
                disabled={updateMutation.isPending}
              >
                Save Settings
              </Button>
            </Box>
          </form>
        </CardContent>
      </Card>

      <Snackbar
        open={showSuccess}
        autoHideDuration={6000}
        onClose={() => setShowSuccess(false)}
      >
        <Alert onClose={() => setShowSuccess(false)} severity="success">
          Settings updated successfully!
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default Settings; 